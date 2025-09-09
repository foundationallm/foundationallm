﻿using FluentValidation;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Authorization;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Authorization.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Authorization resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationServiceClient">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="identityManagementService">The <see cref="IIdentityManagementService"/> providing identity management services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class AuthorizationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationServiceClient,
        IIdentityManagementService identityManagementService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
        : ResourceProviderServiceBase<ResourceReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationServiceClient,
            null,
            null,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AuthorizationResourceProviderService>(),
            [],
            proxyMode: proxyMode)
    {
        private readonly IIdentityManagementService _identityManagementService = identityManagementService;

        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AuthorizationResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Authorization;

        /// <inheritdoc/>
        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> GetResourcesAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.MainResourceTypeName switch
            {
                AuthorizationResourceTypeNames.RoleDefinitions => LoadRoleDefinitions(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private static List<RoleDefinition> LoadRoleDefinitions(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
                return [.. RoleDefinitions.All.Values];
            else
            {
                if (RoleDefinitions.All.TryGetValue(instance.ResourceId, out var roleDefinition))
                    return [roleDefinition];
                else
                    return [];
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(
            ResourcePath resourcePath,
            string? serializedResource,
            ResourceProviderFormFile? formFile,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>

            resourcePath.MainResourceTypeName switch
            {
                AuthorizationResourceTypeNames.RoleAssignments => await UpdateRoleAssignments(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateRoleAssignments(ResourcePath resourcePath, string serializedRoleAssignment, UnifiedUserIdentity userIdentity)
        {
            var roleAssignment = JsonSerializer.Deserialize<RoleAssignment>(serializedRoleAssignment)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != roleAssignment.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            roleAssignment.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var roleAssignmentValidator = _resourceValidatorFactory.GetValidator<RoleAssignment>()!;
            var context = new ValidationContext<object>(roleAssignment);
            var validationResult = await roleAssignmentValidator.ValidateAsync(context);
            if (!validationResult.IsValid)
            {
                throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                    StatusCodes.Status400BadRequest);
            }

            var roleAssignmentResult = await _authorizationServiceClient.CreateRoleAssignment(
                _instanceSettings.Id,
                new RoleAssignmentCreateRequest()
                {
                    Name = roleAssignment.Name,
                    Description = roleAssignment.Description,
                    ObjectId = roleAssignment.ObjectId,
                    PrincipalId = roleAssignment.PrincipalId,
                    PrincipalType = roleAssignment.PrincipalType,
                    RoleDefinitionId = roleAssignment.RoleDefinitionId,
                    Scope = roleAssignment.Scope,
                    CreatedBy = userIdentity.UPN
                },
                userIdentity);

            if (roleAssignmentResult.Success)
                return new ResourceProviderUpsertResult
                {
                    ObjectId = roleAssignment.ObjectId,
                    ResourceExists = false
                };

            if (roleAssignmentResult.ResultReason == RoleAssignmentResultReasons.AssignmentExists)
                throw new ResourceProviderException($"The role assignment already exists.",
                    StatusCodes.Status409Conflict);
            throw new ResourceProviderException("The role assignment failed.");
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case AuthorizationResourceTypeNames.RoleAssignments:
                    await _authorizationServiceClient.DeleteRoleAssignment(
                        _instanceSettings.Id,
                        resourcePath.ResourceId!,
                        userIdentity);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
            resourcePath.ResourceTypeName switch
            {
                AuthorizationResourceTypeNames.RoleAssignments => resourcePath.Action switch
                {
                    ResourceProviderActions.Filter => await FilterRoleAssignments(
                        resourcePath.ResourceTypeInstances[0],
                        serializedAction,
                        userIdentity,
                        requestPayloadValidator: requestPayloadValidator),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                AuthorizationResourceTypeNames.SecurityPrincipals => resourcePath.Action switch
                {
                    ResourceProviderActions.Filter => await FilterSecurityPrincipals(
                        resourcePath,
                        JsonSerializer.Deserialize<SecurityPrincipalQueryParameters>(serializedAction)
                            ?? throw new ResourceProviderException("Invalid query parameters object.",
                                StatusCodes.Status400BadRequest),
                        userIdentity,
                        requestPayloadValidator: requestPayloadValidator),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        #region Helpers for ExecuteActionAsync

        private async Task<List<ResourceProviderGetResult<RoleAssignment>>> FilterRoleAssignments(
            ResourceTypeInstance instance,
            string serializedAction,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null)
        {
            var queryParameters = JsonSerializer.Deserialize<RoleAssignmentQueryParameters>(serializedAction)!;

            if (string.IsNullOrWhiteSpace(queryParameters.Scope))
                throw new ResourceProviderException("Invalid scope. Unable to retrieve role assignments.");
            else if (requestPayloadValidator is not null
                && !requestPayloadValidator(queryParameters))
                throw new ResourceProviderException("The request payload is invalid.",
                    StatusCodes.Status400BadRequest);
            else
            {
                if (queryParameters.SecurityPrincipalIds is not null
                    && queryParameters.SecurityPrincipalIds.Count == 1
                    && queryParameters.SecurityPrincipalIds[0] == SecurityPrincipalVariableNames.CurrentUserIds)
                        queryParameters.SecurityPrincipalIds =
                            [userIdentity.UserId!, .. userIdentity.GroupIds];

                var roleAssignments = (await _authorizationServiceClient.GetRoleAssignments(
                    _instanceSettings.Id, queryParameters, userIdentity))
                    .Where(ra => !ra.Deleted)
                    .ToList();

                if (instance.ResourceId != null)
                {
                    var roleAssignment = roleAssignments.Where(roleAssignment => roleAssignment.ObjectId == instance.ResourceId).SingleOrDefault();

                    if (roleAssignment == null)
                        throw new ResourceProviderException($"Could not locate the {instance.ResourceId} role assignment resource.",
                            StatusCodes.Status404NotFound);
                    else
                        roleAssignments = [roleAssignment];
                }

                return roleAssignments.Select(x => new ResourceProviderGetResult<RoleAssignment>() { Resource = x, Roles = [], Actions = [] }).ToList();
            }
        }

        private async Task<List<ResourceProviderGetResult<SecurityPrincipal>>> FilterSecurityPrincipals(
            ResourcePath resourcePath,
            SecurityPrincipalQueryParameters queryParameters,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null)
        {
            await _validator.ValidateAndThrowAsync<SecurityPrincipalQueryParameters>(queryParameters);

            if (queryParameters.Ids is not null)
            {
                var identityObjects = await _identityManagementService.GetObjectsByIds(
                    new ObjectQueryParameters
                    {
                        Ids = queryParameters.Ids
                    });

                return [.. identityObjects
                .Select(io => new ResourceProviderGetResult<SecurityPrincipal>
                {
                    Resource = new SecurityPrincipal
                    {
                        Id = io.Id!,
                        Type = io.ObjectType,
                        Name = io.DisplayName!,
                        Email = io.Email
                    },
                    Roles = [],
                    Actions = []
                })];
            }

            var identityObjects2 = queryParameters.SecurityPrincipalType switch
            {
                SecurityPrincipalTypes.User => await _identityManagementService.GetUsers(
                    new ObjectQueryParameters
                    {
                        Name = queryParameters.Name,
                        Ids = []
                    }),
                SecurityPrincipalTypes.Group => await _identityManagementService.GetUserGroups(
                    new ObjectQueryParameters
                    {
                        Name = queryParameters.Name,
                        Ids = []
                    }),
                SecurityPrincipalTypes.ServicePrincipal => await _identityManagementService.GetServicePrincipals(
                    new ObjectQueryParameters
                    {
                        Name = queryParameters.Name,
                        Ids = []
                    }),
                _ => throw new ResourceProviderException(
                    "Invalid security principal type. Unable to retrieve security principals.",
                    StatusCodes.Status400BadRequest)
            };

            return [.. identityObjects2.Items!
                .Select(io => new ResourceProviderGetResult<SecurityPrincipal>
                {
                    Resource = new SecurityPrincipal
                    {
                        Id = io.Id!,
                        Type = io.ObjectType,
                        Name = io.DisplayName!,
                        Email = io.Email
                    },
                    Roles = [],
                    Actions = []
                })
            ];
        }

        #endregion

        #endregion
    }
}
