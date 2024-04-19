using FluentValidation;
using FoundationaLLM.Authorization.Constants;
using FoundationaLLM.Authorization.Interfaces;
using FoundationaLLM.Authorization.Models;
using FoundationaLLM.Authorization.Models.Configuration;
using FoundationaLLM.Authorization.ResourceProviders;
using FoundationaLLM.Authorization.Utils;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Authorization.Services
{
    /// <summary>
    /// Implements the core authorization engine.
    /// </summary>
    public class AuthorizationCore : IAuthorizationCore
    {
        private readonly IStorageService _storageService;
        private readonly IResourceValidatorFactory _resourceValidatorFactory;
        private readonly ILogger<AuthorizationCore> _logger;
        private readonly AuthorizationCoreSettings _settings;
        private readonly ConcurrentDictionary<string, RoleAssignmentStore> _roleAssignmentStores = [];
        private readonly ConcurrentDictionary<string, RoleAssignmentCache> _roleAssignmentCaches = [];
        private readonly IValidator<ActionAuthorizationRequest> _actionAuthorizationRequestValidator;

        private const string ROLE_ASSIGNMENTS_CONTAINER_NAME = "role-assignments";
        private bool _initialized = false;
        private readonly object _syncRoot = new();

        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationCore"/> class.
        /// </summary>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
        /// <param name="resourceValidatorFactory"> The resource validator factory used to create resource validators.</param>
        /// <param name="logger">The logger used for logging.</param>
        public AuthorizationCore(
            IOptions<AuthorizationCoreSettings> options,
            IStorageService storageService,
            IResourceValidatorFactory resourceValidatorFactory,
            ILogger<AuthorizationCore> logger)
        {
            _settings = options.Value;
            _storageService = storageService;
            _resourceValidatorFactory = resourceValidatorFactory;
            _logger = logger;

            _actionAuthorizationRequestValidator = _resourceValidatorFactory.GetValidator<ActionAuthorizationRequest>()!;

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _initialized property.
            _ = Task.Run(Initialize);
        }

        private async Task Initialize()
        {
            try
            {
                foreach (var instanceId in _settings.InstanceIds)
                {
                    var roleAssignmentStoreFile = $"/{instanceId.ToLower()}.json";
                    RoleAssignmentStore? roleAssignmentStore;

                    if (await _storageService.FileExistsAsync(ROLE_ASSIGNMENTS_CONTAINER_NAME, roleAssignmentStoreFile, default))
                    {
                        var fileContent = await _storageService.ReadFileAsync(ROLE_ASSIGNMENTS_CONTAINER_NAME, roleAssignmentStoreFile, default);
                        roleAssignmentStore = JsonSerializer.Deserialize<RoleAssignmentStore>(
                            Encoding.UTF8.GetString(fileContent.ToArray()));
                        if (roleAssignmentStore == null
                            || string.Compare(roleAssignmentStore.InstanceId, instanceId) != 0)
                        {
                            _logger.LogError("The role assignment store file for instance {InstanceId} is invalid.", instanceId);
                        }
                        else
                        {
                            _roleAssignmentStores.AddOrUpdate(instanceId, roleAssignmentStore, (k, v) => roleAssignmentStore);
                            _logger.LogInformation("The role assignment store for instance {InstanceId} has been loaded.", instanceId);
                        }
                    }
                    else
                    {
                        roleAssignmentStore = new RoleAssignmentStore
                        {
                            InstanceId = instanceId,
                            RoleAssignments = []
                        };

                        _roleAssignmentStores.AddOrUpdate(instanceId, roleAssignmentStore, (k, v) => roleAssignmentStore);
                        await _storageService.WriteFileAsync(
                            ROLE_ASSIGNMENTS_CONTAINER_NAME,
                            roleAssignmentStoreFile,
                            JsonSerializer.Serialize(roleAssignmentStore),
                            default,
                            default);
                        _logger.LogInformation("The role assignment store for instance {InstanceId} has been created.", instanceId);
                    }

                    if (roleAssignmentStore != null)
                    {
                        roleAssignmentStore.EnrichRoleAssignments();
                        _roleAssignmentCaches.AddOrUpdate(instanceId, new RoleAssignmentCache(_roleAssignmentStores[instanceId]), (k, v) => v);
                    }
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The authorization core failed to initialize.");
            }
        }

        /// <inheritdoc/>
        public ActionAuthorizationResult ProcessAuthorizationRequest(string instanceId, ActionAuthorizationRequest authorizationRequest)
        {
            try
            {
                _logger.LogDebug("Authorization request: {AuthorizationRequest}",
                    JsonSerializer.Serialize(authorizationRequest));

                if (!_initialized)
                {
                    _logger.LogError("The authorization core is not initialized.");
                    return new ActionAuthorizationResult { Authorized = false };
                }

                // Basic validation
                _actionAuthorizationRequestValidator.ValidateAndThrow(authorizationRequest);

                var resourcePath = ResourcePathUtils.ParseForAuthorizationRequestResourcePath(
                    authorizationRequest.ResourcePath, _settings.InstanceIds);

                if (string.IsNullOrWhiteSpace(resourcePath.InstanceId)
                    || resourcePath.InstanceId.ToLower().CompareTo(instanceId.ToLower()) != 0)
                {
                    _logger.LogError("The instance id from the controller route and the instance id from the authorization request do not match.");
                    return new ActionAuthorizationResult { Authorized = false };
                }

                return new ActionAuthorizationResult
                {
                    Authorized = ActionAllowed(resourcePath, authorizationRequest)
                };
            }
            catch (Exception ex)
            {
                // If anything goes wrong, we default to denying the request.

                _logger.LogError(ex, "The authorization core failed to process the authorization request.");
                return new ActionAuthorizationResult { Authorized = false };
            }
        }

        public bool AllowAuthorizationRequestsProcessing(string instanceId, string securityPrincipalId)
        {
            var resourcePath = $"/instances/{instanceId}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}";
            _ = ResourcePath.TryParse(
                resourcePath,
                [ ResourceProviderNames.FoundationaLLM_Authorization ],
                AuthorizationResourceProviderMetadata.AllowedResourceTypes,
                false,
                out ResourcePath? parsedResourcePath);
            return ActionAllowed(parsedResourcePath!, new ActionAuthorizationRequest
            {
                Action = AuthorizableActionNames.FoundationaLLM_Authorization_RoleAssignments_Read,
                PrincipalId = securityPrincipalId,
                ResourcePath = resourcePath
            });
        }

        private bool ActionAllowed(ResourcePath resourcePath, ActionAuthorizationRequest authorizationRequest)
        {
            // Get cache associated with the instance id.
            if (_roleAssignmentCaches.TryGetValue(resourcePath.InstanceId!, out var roleAssignmentCache))
            {
                // Combine the principal id and security group ids into one list.
                var objectIds = new List<string> { authorizationRequest.PrincipalId };
                if (authorizationRequest.SecurityGroupIds != null)
                    objectIds.AddRange(authorizationRequest.SecurityGroupIds);

                foreach (var objectId in objectIds)
                {
                    // Retrieve all role assignments associated with the id.
                    var roleAssignments = roleAssignmentCache.GetRoleAssignments(objectId);
                    foreach (var roleAssignment in roleAssignments)
                    {
                        // Retrieve the role definition object
                        if (RoleDefinitions.All.TryGetValue(roleAssignment.RoleDefinitionId, out var roleDefinition))
                        {
                            // Check if the scope of the role assignment includes the resource.
                            // Check if the actions of the role definition include the requested action.
                            if (resourcePath.IncludesResourcePath(roleAssignment.ScopeResourcePath!)
                                && roleAssignment.AllowedActions.Contains(authorizationRequest.Action))
                            {
                                return true;
                            }
                        }
                        else
                            _logger.LogWarning("The role assignment {RoleAssignmentName} references the role definition {RoleDefinitionId} which is invalid.",
                                roleAssignment.Name, roleAssignment.RoleDefinitionId);
                    }
                }
            }

            _logger.LogWarning("The action {ActionName} is not allowed on the resource {ResourcePath} for the principal {PrincipalId}.",
                authorizationRequest.Action,
                resourcePath,
                authorizationRequest.PrincipalId);

            return false;
        }
    }
}
