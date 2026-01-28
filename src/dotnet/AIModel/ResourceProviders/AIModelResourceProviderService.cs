using FluentValidation;
using FoundationaLLM.AIModel.Models;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.AIModel.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.AIModel resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class AIModelResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
        : ResourceProviderServiceBase<AIModelReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AIModelResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AIModelResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_AIModel;

        /// <inheritdoc/>
        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.MainResourceTypeName switch
            {
                AIModelResourceTypeNames.AIModels => await LoadResources<AIModelBase>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath,
                    }),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

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
                AIModelResourceTypeNames.AIModels => await UpdateAIModel(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateAIModel(ResourcePath resourcePath, string serializedAIModel, UnifiedUserIdentity userIdentity)
        {

            var aiModel = JsonSerializer.Deserialize<AIModelBase>(serializedAIModel)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var existingAIModelReference = await _resourceReferenceStore!.GetResourceReference(aiModel.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != aiModel.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var aiModelReference = new AIModelReference
            {
                Name = aiModel.Name!,
                Type = aiModel.Type!,
                Filename = $"/{_name}/{aiModel.Name}.json",
                Deleted = false
            };

            aiModel.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(aiModelReference.ResourceType);
            if (validator is IValidator aiModelValidator)
            {
                var context = new ValidationContext<object>(aiModel);
                var validationResult = await aiModelValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            UpdateBaseProperties(aiModel, userIdentity, isNew: existingAIModelReference is null);
            if (existingAIModelReference is null)
                await CreateResource<AIModelBase>(aiModelReference, aiModel);
            else
                await SaveResource<AIModelBase>(existingAIModelReference, aiModel);

            return new ResourceProviderUpsertResult
            {
                ObjectId = aiModel!.ObjectId,
                ResourceExists = existingAIModelReference is not null
            };
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
            resourcePath.ResourceTypeName switch
            {
                AIModelResourceTypeNames.AIModels => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckResourceName<AIModelBase>(
                        JsonSerializer.Deserialize<ResourceName>(serializedAction)!),
                    ResourceProviderActions.Purge => await PurgeResource<AgentBase>(resourcePath),
                    ResourceProviderActions.Filter => await FilterResources<AIModelBase>(
                        resourcePath,
                        JsonSerializer.Deserialize<ResourceFilter>(serializedAction)!,
                        authorizationResult),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case AIModelResourceTypeNames.AIModels:
                    await DeleteResource<AIModelBase>(resourcePath);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
            await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
        }

        /// <inheritdoc/>
        protected override Dictionary<
            string,
            (
                string ResourceProviderName,
                Func<IResourceProviderService, string, string, UnifiedUserIdentity, Task<ResourceBase?>> BuildInstanceAsync,
                 Dictionary<
                    string,
                    Func<ResourcePath, string, ResourceBase, UnifiedUserIdentity, Task<bool>>
                > InstanceValidators
            )> _parentResourceFactory => new()
            {
                {
                    AgentResourceTypeNames.Agents, // The parent resource type name (e.g., "agents")
                    (
                        // 1. The name of the resource provider that manages this parent resource type.
                        ResourceProviderNames.FoundationaLLM_Agent,
                        // 2. Async function to build/load the parent resource instance
                        async (resourceProviderService, instanceId, resourceName, userIdentity) =>
                            await resourceProviderService.GetResourceAsync<AgentBase>(
                                instanceId,
                                resourceName,
                                userIdentity),
                        // 3. Dictionary of validators: maps resource type -> validation function
                        new()
                        {
                            {
                                AIModelResourceTypeNames.AIModels,
                                async (resourcePath, actionType, parentResourceInstance, userIdentity) =>
                                {
                                    // Validate that the parent (agent) actually references this AI model.
                                    if (parentResourceInstance is AgentBase agent)
                                        return
                                            agent.HasResourceReference(resourcePath.ObjectId!)
                                            && actionType == AuthorizableOperations.Read;

                                    return false;
                                }
                            }
                        }
                    )
                }
            };

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null,
            ResourceBase? parentResourceInstance = null) =>
            (await LoadResource<T>(resourcePath.ResourceId!))!;

        #endregion
    }
}
