using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProvider;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.Events;
using Microsoft.Extensions.Logging;

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using FoundationaLLM.Common.Validation;

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

        /// <summary>
        /// Creates a new instance of the resource provider.
        /// </summary>
        /// <param name="instanceSettings">The <see cref="InstanceSettings"/> that provides instance-wide settings.</param>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services to the resource provider.</param>
        /// <param name="eventService">The <see cref="IEventService"/> providing event services to the resource provider.</param>
        /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing services to instantiate resource validators.</param>
        /// <param name="logger">The logger used for logging.</param>
        /// <param name="eventNamespacesToSubscribe">The list of Event Service event namespaces to subscribe to for local event processing.</param>
        public ResourceProviderServiceBase(
            InstanceSettings instanceSettings,
            IStorageService storageService,
            IEventService eventService,
            IResourceValidatorFactory resourceValidatorFactory,
            ILogger logger,
            List<string>? eventNamespacesToSubscribe = default)
        {
            _storageService = storageService;
            _eventService = eventService;
            _resourceValidatorFactory = resourceValidatorFactory;
            _logger = logger;
            _instanceSettings = instanceSettings;
            _eventNamespacesToSubscribe = eventNamespacesToSubscribe;

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _isInitialized property.
            _ = Task.Run(Initialize);
        }

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

        #region IManagementProviderService

        /// <inheritdoc/>
        public async Task<object> HandleGetAsync(string resourcePath)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath, allowAction: false);
            return await GetResourcesAsyncInternal(instances);
        }

        /// <inheritdoc/>
        public async Task<object> HandlePostAsync(string resourcePath, string serializedResource)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            if (instances.Last().Action != null)
                return await ExecuteActionAsync(instances, serializedResource);
            else
                return await UpsertResourceAsync(instances, serializedResource);
        }

        /// <inheritdoc/>
        public async Task HandleDeleteAsync(string resourcePath)
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath, allowAction: false);
            await DeleteResourceAsync(instances);
        }

        #region Virtuals to be overriden in derived classes

        /// <summary>
        /// The internal implementation of GetResourcesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<object> GetResourcesAsyncInternal(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <param name="serializedResource">The serialized resource being created or updated.</param>
        /// <returns></returns>
        protected virtual async Task<object> UpsertResourceAsync(List<ResourceTypeInstance> instances, string serializedResource)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of ExecuteActionAsync. Must be overriden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <param name="serializedAction">The serialized details of the action being executed.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<object> ExecuteActionAsync(List<ResourceTypeInstance> instances, string serializedAction)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task DeleteResourceAsync(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region IResourceProviderService

        /// <inheritdoc/>
        public IList<T> GetResources<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return GetResourcesInternal<T>(instances);
        }

        /// <inheritdoc/>
        public async Task<IList<T>> GetResourcesAsync<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return await GetResourcesAsyncInternal<T>(instances);
        }

        /// <inheritdoc/>
        public T GetResource<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return GetResourceInternal<T>(instances);
        }

        /// <inheritdoc/>
        public async Task<T> GetResourceAsync<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            return await GetResourceAsyncInternal<T>(instances);
        }

        /// <inheritdoc/>
        public async Task<string> UpsertResourceAsync<T>(string resourcePath, T resource) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            await UpsertResourceAsync<T>(instances, resource);
            return GetObjectId(instances);
        }

        /// <inheritdoc/>
        public string UpsertResource<T>(string resourcePath, T resource) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            UpsertResource<T>(instances, resource);
            return GetObjectId(instances);
        }    

        /// <inheritdoc/>
        public async Task DeleteResourceAsync<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            await DeleteResourceAsync<T>(instances);
        }

        /// <inheritdoc/>
        public void DeleteResource<T>(string resourcePath) where T : class
        {
            if (!_isInitialized)
                throw new ResourceProviderException($"The resource provider {_name} is not initialized.");
            var instances = GetResourceInstancesFromPath(resourcePath);
            DeleteResource<T>(instances);
        }

        #endregion

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
        /// Gets the resource types dictionary. Must be overriden in derived classes.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of ExecuteAction. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<ResourceProviderActionResult> ExecuteActionInternal(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of GetResources. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual IList<T> GetResourcesInternal<T>(List<ResourceTypeInstance> instances) where T : class =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of GetResourcesAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<IList<T>> GetResourcesAsyncInternal<T>(List<ResourceTypeInstance> instances) where T : class
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of GetResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual T GetResourceInternal<T>(List<ResourceTypeInstance> instances) where T : class =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of GetResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task<T> GetResourceAsyncInternal<T>(List<ResourceTypeInstance> instances) where T : class
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of UpsertResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <returns></returns>
        protected virtual void UpsertResource<T>(List<ResourceTypeInstance> instances, T resource) =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of UpsertResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <returns></returns>
        protected virtual async Task UpsertResourceAsync<T>(List<ResourceTypeInstance> instances, T resource)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal implementation of DeleteResource. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual void DeleteResource<T>(List<ResourceTypeInstance> instances) =>
            throw new NotImplementedException();

        /// <summary>
        /// The internal implementation of DeleteResourceAsync. Must be overridden in derived classes.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns></returns>
        protected virtual async Task DeleteResourceAsync<T>(List<ResourceTypeInstance> instances)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the resource unique identifier based on the resource path.
        /// </summary>
        /// <param name="instances">The list of <see cref="ResourceTypeInstance"/> objects parsed from the resource path.</param>
        /// <returns>The unique resource identifier.</returns>
        /// <exception cref="ResourceProviderException"></exception>
        protected string GetObjectId(List<ResourceTypeInstance> instances)
        {
            foreach (var instance in instances)
                if (string.IsNullOrWhiteSpace(instance.ResourceType)
                    || string.IsNullOrWhiteSpace(instance.ResourceId)
                    || !(instance.Action == null))
                    throw new ResourceProviderException("The provided resource path is not a valid resource identifier.",
                        StatusCodes.Status400BadRequest);

            return $"/instances/{_instanceSettings.Id}/providers/{_name}/{string.Join("/",
                instances.Select(i => $"{i.ResourceType}/{i.ResourceId}").ToArray())}";
        }

        private List<ResourceTypeInstance> GetResourceInstancesFromPath(string resourcePath, bool allowAction = true)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
                throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.",
                    StatusCodes.Status400BadRequest);

            var tokens = resourcePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var result = new List<ResourceTypeInstance>();
            var currentResourceTypes = GetResourceTypes();
            var currentIndex = 0;
            while (currentIndex < tokens.Length)
            {
                if (currentResourceTypes == null
                    || !currentResourceTypes.TryGetValue(tokens[currentIndex], out ResourceTypeDescriptor? currentResourceType))
                    throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.",
                        StatusCodes.Status400BadRequest);

                var resourceTypeInstance = new ResourceTypeInstance(tokens[currentIndex]);
                result.Add(resourceTypeInstance);

                if (currentIndex + 1 == tokens.Length)
                    // No more tokens left, which means we have a resource type instance without actions or subtypes.
                    // This will be used by resource providers to retrieve all resources of a specific resource type.
                    break;

                // Check if the next token is an action or a resource id.
                // The only way to determine is by matching the token with the list of supported actions.
                // If there is not match, the token is considered to be a resource identifier.
                if (currentResourceType.Actions.Any(a => a.Name == tokens[currentIndex + 1]))
                {
                    // The next token is an action
                    if (!allowAction)
                        throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.",
                            StatusCodes.Status400BadRequest);

                    resourceTypeInstance.Action = tokens[currentIndex + 1];

                    // It must be the last token
                    if (currentIndex + 2 == tokens.Length)
                        break;
                    else
                        throw new ResourceProviderException($"An action must be the last token in a resource path.",
                            StatusCodes.Status400BadRequest);
                }
                else
                    // The next token is a resource identifier
                    resourceTypeInstance.ResourceId = tokens[currentIndex + 1];

                if (currentIndex + 2 == tokens.Length - 1)
                {
                    // Only one token left after the resource identifier.
                    // This means it can only be an action.
                    if (currentResourceType.Actions.Any(a => a.Name == tokens[currentIndex + 2]))
                    {
                        // The token represents an action.
                        if (!allowAction)
                            throw new ResourceProviderException($"The resource path [{resourcePath}] is invalid.",
                                StatusCodes.Status400BadRequest);

                        resourceTypeInstance.Action = tokens[currentIndex + 2];
                        break;
                    }
                    else
                        throw new ResourceProviderException($"The [{tokens[currentIndex + 2]}] action is invalid.",
                            StatusCodes.Status400BadRequest);
                }

                currentResourceTypes = currentResourceType.SubTypes;
                currentIndex += 2;
            }

            return result;
        }

        #region Events handling

        /// <summary>
        /// Handles events received from the <see cref="IEventService"/> when they are dequeued locally.
        /// </summary>
        /// <param name="e">The <see cref="EventSetEventArgs"/> containing the events namespace and the actual events.</param>
        /// <returns></returns>
        protected virtual async Task HandleEvents(EventSetEventArgs e) =>
            await Task.CompletedTask;

        #endregion
    }
}
