﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.ResourceProviders
{
    /// <summary>
    /// Implements basic resource provider functionality
    /// </summary>
    public class ResourceProviderServiceBase : IResourceProviderService
    {
        private bool _isInitialized = false;

        private LocalEventService? _localEventService;
        private readonly List<string>? _eventNamespacesToSubscribe;
        private readonly ImmutableList<string> _allowedResourceProviders;
        private readonly Dictionary<string, ResourceTypeDescriptor> _allowedResourceTypes;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviders = [];

        /// <summary>
        /// The <see cref="IServiceProvider"/> tha provides dependency injection services.
        /// </summary>
        protected readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// The <see cref="IAuthorizationService"/> providing authorization services to the resource provider.
        /// </summary>
        protected readonly IAuthorizationService _authorizationService;

        /// <summary>
        /// The <see cref="IStorageService"/> providing storage services to the resource provider.
        /// </summary>
        protected readonly IStorageService _storageService;

        /// <summary>
        /// The <see cref="IEventService"/> providing event services to the resource provider.
        /// </summary>
        protected readonly IEventService _eventService;

        /// <summary>
        /// The <see cref="IResourceValidatorFactory"/> providing services to instantiate resource validators.
        /// </summary>
        protected readonly IResourceValidatorFactory _resourceValidatorFactory;

        /// <summary>
        /// The logger used for logging.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// The <see cref="InstanceSettings"/> that provides instance-wide settings.
        /// </summary>
        protected readonly InstanceSettings _instanceSettings;

        /// <summary>
        /// The name of the storage container name used by the resource provider to store its internal data.
        /// </summary>
        protected virtual string _storageContainerName => "resource-provider";

        /// <summary>
        /// The name of the resource provider. Must be overridden in derived classes.
        /// </summary>
        protected virtual string _name => throw new NotImplementedException();

        /// <summary>
        /// Default JSON serialization settings.
        /// </summary>
        protected virtual JsonSerializerOptions _serializerSettings => new()
        {
            WriteIndented = true
        };

        /// <inheritdoc/>
        public string Name => _name;

        /// <inheritdoc/>
        public bool IsInitialized  => _isInitialized;

        /// <inheritdoc/>
        public Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes => _allowedResourceTypes;

        /// <inheritdoc/>
        public string StorageAccountName => _storageService.StorageAccountName;

        /// <summary>
        /// Creates a new instance of the resource provider.
        /// </summary>
        /// <param name="instanceSettings">The <see cref="InstanceSettings"/> that provides instance-wide settings.</param>
        /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services to the resource provider.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services to the resource provider.</param>
        /// <param name="eventService">The <see cref="IEventService"/> providing event services to the resource provider.</param>
        /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing services to instantiate resource validators.</param>
        /// <param name="logger">The logger used for logging.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
        /// <param name="eventNamespacesToSubscribe">The list of Event Service event namespaces to subscribe to for local event processing.</param>
        public ResourceProviderServiceBase(
            InstanceSettings instanceSettings,
            IAuthorizationService authorizationService,
            IStorageService storageService,
            IEventService eventService,
            IResourceValidatorFactory resourceValidatorFactory,
            IServiceProvider serviceProvider,
            ILogger logger,
            List<string>? eventNamespacesToSubscribe = default)
        {
            _authorizationService = authorizationService;
            _storageService = storageService;
            _eventService = eventService;
            _resourceValidatorFactory = resourceValidatorFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _instanceSettings = instanceSettings;
            _eventNamespacesToSubscribe = eventNamespacesToSubscribe;

            _allowedResourceProviders = [_name];
            _allowedResourceTypes = GetResourceTypes();

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _isInitialized property.
            _ = Task.Run(Initialize);
        }

        #region Initialization

        /// <inheritdoc/>
        private async Task Initialize()
        {
            try
            {
                await InitializeInternal();

                if (_eventNamespacesToSubscribe != null
                    && _eventNamespacesToSubscribe.Count > 0)
                {
                    _localEventService = new LocalEventService(
                        new LocalEventServiceSettings { EventProcessingCycleSeconds = 10 },
                        _eventService,
                        _logger);
                    _localEventService.SubscribeToEventNamespaces(_eventNamespacesToSubscribe);
                    _localEventService.StartLocalEventProcessing(HandleEvents);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The resource provider {ResourceProviderName} failed to initialize.", _name);
            }
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of Initialize. Must be overridden in derived classes.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task InitializeInternal()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the details about the resource types managed by the resource provider.
        /// </summary>
        /// <returns>A dictionary of <see cref="ResourceTypeDescriptor"/> objects with details about the resource types.</returns>
        protected virtual Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() => [];

        #endregion

        #endregion

        #region IManagementProviderService

        /// <inheritdoc/>
        public async Task<object> HandleGetAsync(string resourcePath, UnifiedUserIdentity userIdentity)
        {
            EnsureServiceInitialization();
            var parsedResourcePath = EnsureValidResourcePath(resourcePath, HttpMethod.Get, false);

            if (!parsedResourcePath.IsResourceTypePath)
            {
                // Authorize access to the resource path.
                await Authorize(parsedResourcePath, userIdentity, "read");
            }
           
            return await GetResourcesAsync(parsedResourcePath, userIdentity);
        }

        /// <inheritdoc/>
        public async Task<object> HandlePostAsync(string resourcePath, string serializedResource, UnifiedUserIdentity userIdentity)
        {
            EnsureServiceInitialization();
            var parsedResourcePath = EnsureValidResourcePath(resourcePath, HttpMethod.Post, true);

            // Authorize access to the resource path.
            await Authorize(parsedResourcePath, userIdentity, "write");

            if (parsedResourcePath.ResourceTypeInstances.Last().Action != null)
                return await ExecuteActionAsync(parsedResourcePath, serializedResource, userIdentity);
            else
            {
                var resource = await UpsertResourceAsync(parsedResourcePath, serializedResource, userIdentity);

                var upsertResult = resource as ResourceProviderUpsertResult;

                if (upsertResult!.ResourceExists == false && Name != ResourceProviderNames.FoundationaLLM_Authorization)
                {
                    var roleAssignmentName = Guid.NewGuid().ToString();
                    var roleAssignmentDescription = $"Owner role for {userIdentity.Name}";
                    var roleAssignmentResult = await _authorizationService.ProcessRoleAssignmentRequest(
                        _instanceSettings.Id,
                        new RoleAssignmentRequest()
                        {
                            Name = roleAssignmentName,
                            Description = roleAssignmentDescription,
                            ObjectId = $"/instances/{_instanceSettings.Id}/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleAssignments}/{roleAssignmentName}",
                            PrincipalId = userIdentity.UserId!,
                            PrincipalType = PrincipalTypes.User,
                            RoleDefinitionId = $"/providers/{ResourceProviderNames.FoundationaLLM_Authorization}/{AuthorizationResourceTypeNames.RoleDefinitions}/{RoleDefinitionNames.Owner}",
                            Scope = upsertResult!.ObjectId ?? throw new ResourceProviderException($"The {roleAssignmentDescription} could not be assigned. Could not set the scope for the resource.")
                        },
                        userIdentity);

                    if (!roleAssignmentResult.Success)
                        _logger.LogError("The {RoleAssignment} could not be assigned.", roleAssignmentDescription);
                }

                return resource;
            }
        }

        /// <inheritdoc/>
        public async Task HandleDeleteAsync(string resourcePath, UnifiedUserIdentity userIdentity)
        {
            EnsureServiceInitialization();
            var parsedResourcePath = EnsureValidResourcePath(resourcePath, HttpMethod.Delete, false);

            // Authorize access to the resource path.
            await Authorize(parsedResourcePath, userIdentity, "delete");

            await DeleteResourceAsync(parsedResourcePath, userIdentity);
        }

        /// <summary>
        /// Gets a <see cref="ResourcePath"/> object for the specified string resource path.
        /// </summary>
        /// <param name="resourcePath">The resource path.</param>
        /// <param name="allowAction">Indicates whether actions are allowed in the resource path.</param>
        /// <returns>A <see cref="ResourcePath"/> object.</returns>
        public ResourcePath GetResourcePath(string resourcePath, bool allowAction = true) =>
            new(
                resourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes,
                allowAction: allowAction);

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of GetResourcesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns></returns>
        protected virtual async Task<object> GetResourcesAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="serializedResource">The serialized resource being created or updated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns></returns>
        protected virtual async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of ExecuteActionAsync. Must be overriden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="serializedAction">The serialized details of the action being executed.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> with details about the identity of the user.</param>
        /// <returns></returns>
        protected virtual async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region IResourceProviderService

        /// <inheritdoc/>
        public async Task<T> GetResource<T>(string resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderOptions? options = null) where T : class
        {
            EnsureServiceInitialization();
            var parsedResourcePath = EnsureValidResourcePath(resourcePath, HttpMethod.Get, false, typeof(T));

            // Authorize access to the resource path.
            await Authorize(parsedResourcePath, userIdentity, "read");

            return await GetResourceInternal<T>(parsedResourcePath, userIdentity, options);
        }

        /// <inheritdoc/>
        public async Task<TResult> UpsertResourceAsync<T, TResult>(string resourcePath, T resource, UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult
        {
            EnsureServiceInitialization();
            var parsedResourcePath = EnsureValidResourcePath(resourcePath, HttpMethod.Post, false, typeof(T));

            // Authorize access to the resource path.
            await Authorize(parsedResourcePath, userIdentity, "write");

            return await UpsertResourceAsyncInternal<T, TResult>(parsedResourcePath, resource, userIdentity);
        }

        #region Virtuals to override in derived classes

        /// <summary>
        /// The internal implementation of GetResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <param name="options">The <see cref="ResourceProviderOptions"/> which provides operation parameters.</param>
        /// <returns></returns>
        protected virtual async Task<T> GetResourceInternal<T>(ResourcePath resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderOptions? options = null) where T : class
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <typeparam name="T">The type of the resource being created or updated.</typeparam>
        /// <typeparam name="TResult">The type of the result returned.</typeparam>
        /// <param name="resourcePath">A <see cref="ResourcePath"/> containing information about the resource path.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing information about the calling user identity.</param>
        /// <returns></returns>
        protected virtual async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(ResourcePath resourcePath, T resource, UnifiedUserIdentity userIdentity)
            where T : ResourceBase
            where TResult : ResourceProviderUpsertResult
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Authorization

        /// <summary>
        /// Authorizes the specified action on a resource path.
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> containing information about the identity of the user.</param>
        /// <param name="actionType"></param>
        /// <returns></returns>
        /// <exception cref="ResourceProviderException"></exception>
        private async Task Authorize(ResourcePath resourcePath, UnifiedUserIdentity? userIdentity, string actionType)
        {
            try
            {
                if (userIdentity == null
                    || userIdentity.UserId == null)
                    throw new Exception("The provided user identity information cannot be used for authorization.");

                var rp = resourcePath.GetObjectId(_instanceSettings.Id, _name);
                var result = await _authorizationService.ProcessAuthorizationRequest(
                    _instanceSettings.Id,
                    $"{_name}/{resourcePath.MainResourceType!}/{actionType}",
                    [rp],
                    userIdentity);

                if (!result.AuthorizationResults[rp])
                    throw new AuthorizationException("Access is not authorized.");
            }
            catch (AuthorizationException)
            {
                _logger.LogWarning("The {ActionType} access to the resource path {ResourcePath} was not authorized for user {UserName} : userId {UserId}.",
                    actionType, resourcePath.GetObjectId(_instanceSettings.Id, _name), userIdentity!.Username, userIdentity!.UserId);
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while attempting to authorize access to the resource path.");
                throw new ResourceProviderException(
                    "An error occurred while attempting to authorize access to the resource path.",
                    StatusCodes.Status403Forbidden);
            }
        }

        #endregion

        #region Events handling

        /// <summary>
        /// Handles events received from the <see cref="IEventService"/> when they are dequeued locally.
        /// </summary>
        /// <param name="e">The <see cref="EventSetEventArgs"/> containing the events namespace and the actual events.</param>
        /// <returns></returns>
        protected virtual async Task HandleEvents(EventSetEventArgs e) =>
            await Task.CompletedTask;

        #endregion

        #region Internal validation

        private void EnsureServiceInitialization()
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
        }

        private ResourcePath EnsureValidResourcePath(string resourcePath, HttpMethod operationType, bool allowAction = true, Type? resourceType = null)
        {
            var parsedResourcePath = new ResourcePath(
                resourcePath,
                _allowedResourceProviders,
                _allowedResourceTypes,
                allowAction: allowAction);

            var mainResourceType = parsedResourcePath.MainResourceType
                ?? throw new ResourceProviderException(
                    $"The resource path {resourcePath} does not have a main resource type and cannot be handled by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);

            if (!AllowedResourceTypes.TryGetValue(mainResourceType, out ResourceTypeDescriptor? resourceTypeDescriptor))
                throw new ResourceProviderException(
                    $"The resource type {mainResourceType} cannot be handled by the {_name} resource provider",
                    StatusCodes.Status400BadRequest);

            if (operationType.Method == HttpMethods.Post)
            {
                if (resourceType != null
                    && !resourceTypeDescriptor.AllowedTypes.Single(at => at.HttpMethod == operationType.Method).AllowedBodyTypes.Contains(resourceType))
                    throw new ResourceProviderException(
                        $"The type {nameof(resourceType)} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            }

            return parsedResourcePath;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Gets a resource provider service by name.
        /// </summary>
        /// <param name="name">The name of the resource provider.</param>
        /// <returns>The <see cref="IResourceProviderService"/> used to interact with the resource provider.</returns>
        protected IResourceProviderService GetResourceProviderServiceByName(string name)
        {
            if (!_resourceProviders.ContainsKey(name))
                _resourceProviders.Add(
                    name,
                    _serviceProvider.GetRequiredService<IEnumerable<IResourceProviderService>>()
                        .Single(rp => rp.Name == name));
            return _resourceProviders[name];
        }

        /// <summary>
        /// Updates the base properties of an object derived from <see cref="ResourceBase"/>.
        /// </summary>
        /// <param name="resource">The <see cref="ResourceBase"/> object to be updated.</param>
        /// <param name="userIdentity">The <see cref="UnifiedUserIdentity"/> providing the information about the identity of the user that performed a create or update operation on the resource.</param>
        protected void UpdateBaseProperties(ResourceBase resource, UnifiedUserIdentity userIdentity)
        {
            if (string.IsNullOrWhiteSpace(resource.CreatedBy))
            {
                // The resource was just created
                resource.CreatedBy = userIdentity.UPN ?? userIdentity.UserId ?? "N/A";
                resource.CreatedOn = DateTimeOffset.UtcNow;
            }
            else
            {
                // The resource was updated
                resource.UpdatedBy = userIdentity.UPN ?? userIdentity.UserId ?? "N/A";
                resource.UpdatedOn = DateTimeOffset.UtcNow;
            }
        }

        #endregion
    }
}
