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
using FoundationaLLM.Common.Models.ResourceProviders.Infrastructure;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Infrastructure.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Infrastructure resource provider.
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
    public class InfrastructureResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Infrastructure)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
        : ResourceProviderServiceBase<InfrastructureReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<InfrastructureResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            InfrastructureResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Infrastructure;

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
                InfrastructureResourceTypeNames.AzureContainerAppsEnvironments => resourcePath.HasSubordinateResourceId
                    ? await LoadResources<AzureContainerApp>(
                        resourcePath.ResourceTypeInstances[1],
                        authorizationResult,
                        options ?? new ResourceProviderGetOptions
                        {
                            IncludeRoles = resourcePath.IsResourceTypePath,
                        })
                    : await LoadResources<AzureContainerAppsEnvironment>(
                        resourcePath.ResourceTypeInstances[0],
                        authorizationResult,
                        options ?? new ResourceProviderGetOptions
                        {
                            IncludeRoles = resourcePath.IsResourceTypePath,
                        }),
                InfrastructureResourceTypeNames.AzureKubernetesServices => resourcePath.HasSubordinateResourceId
                    ? await LoadResources<AzureKubernetesServiceDeployment>(
                        resourcePath.ResourceTypeInstances[1],
                        authorizationResult,
                        options ?? new ResourceProviderGetOptions
                        {
                            IncludeRoles = resourcePath.IsResourceTypePath,
                        })
                    : await LoadResources<AzureKubernetesService>(
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

            resourcePath.ResourceTypeName switch
            {
                InfrastructureResourceTypeNames.AzureContainerApps => await UpdateAzureContainerApp(resourcePath, serializedResource!, userIdentity),
                InfrastructureResourceTypeNames.AzureKubernetesServiceDeployments => await UpdateAzureKubernetesServiceDeployment(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateAzureContainerApp(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity)
        {
            var containerApp = JsonSerializer.Deserialize<AzureContainerApp>(serializedResource)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var existingReference = await _resourceReferenceStore!.GetResourceReference(containerApp.Name!);

            if (resourcePath.ResourceId != containerApp.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var reference = new InfrastructureReference
            {
                Name = containerApp.Name!,
                Type = InfrastructureTypes.AzureContainerApp,
                Filename = $"/{_name}/{InfrastructureResourceTypeNames.AzureContainerAppsEnvironments}/{resourcePath.MainResourceId}/{containerApp.Name}.json",
                Deleted = false
            };

            containerApp.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            UpdateBaseProperties(containerApp, userIdentity, isNew: existingReference is null);
            if (existingReference is null)
                await CreateResource<AzureContainerApp>(reference, containerApp);
            else
                await SaveResource<AzureContainerApp>(existingReference, containerApp);

            return new ResourceProviderUpsertResult
            {
                ObjectId = containerApp!.ObjectId,
                ResourceExists = existingReference is not null
            };
        }

        private async Task<ResourceProviderUpsertResult> UpdateAzureKubernetesServiceDeployment(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity)
        {
            var deployment = JsonSerializer.Deserialize<AzureKubernetesServiceDeployment>(serializedResource)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var existingReference = await _resourceReferenceStore!.GetResourceReference(deployment.Name!);

            if (resourcePath.ResourceId != deployment.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var reference = new InfrastructureReference
            {
                Name = deployment.Name!,
                Type = InfrastructureTypes.AzureKubernetesServiceDeployment,
                Filename = $"/{_name}/{InfrastructureResourceTypeNames.AzureKubernetesServices}/{resourcePath.MainResourceId}/{deployment.Name}.json",
                Deleted = false
            };

            deployment.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            UpdateBaseProperties(deployment, userIdentity, isNew: existingReference is null);
            if (existingReference is null)
                await CreateResource<AzureKubernetesServiceDeployment>(reference, deployment);
            else
                await SaveResource<AzureKubernetesServiceDeployment>(existingReference, deployment);

            return new ResourceProviderUpsertResult
            {
                ObjectId = deployment!.ObjectId,
                ResourceExists = existingReference is not null
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
                InfrastructureResourceTypeNames.AzureContainerApps => resourcePath.Action switch
                {
                    ResourceProviderActions.Restart => await RestartAzureContainerApp(resourcePath, userIdentity),
                    ResourceProviderActions.Scale => await ScaleAzureContainerApp(resourcePath, serializedAction, userIdentity),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                InfrastructureResourceTypeNames.AzureKubernetesServiceDeployments => resourcePath.Action switch
                {
                    ResourceProviderActions.Restart => await RestartAzureKubernetesServiceDeployment(resourcePath, userIdentity),
                    ResourceProviderActions.Scale => await ScaleAzureKubernetesServiceDeployment(resourcePath, serializedAction, userIdentity),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Action helpers

        private async Task<ResourceProviderActionResult> RestartAzureContainerApp(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            // TODO: Implement actual Azure Container App restart logic using Azure SDK
            _logger.LogInformation("Restart action triggered for Azure Container App {ContainerAppName}", resourcePath.ResourceId);
            await Task.CompletedTask;
            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        private async Task<ResourceProviderActionResult> ScaleAzureContainerApp(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity)
        {
            var scaleRequest = JsonSerializer.Deserialize<InfrastructureScaleRequest>(serializedAction)
                ?? throw new ResourceProviderException("The scale request is invalid.",
                    StatusCodes.Status400BadRequest);

            // TODO: Implement actual Azure Container App scale logic using Azure SDK
            _logger.LogInformation("Scale action triggered for Azure Container App {ContainerAppName} with replicas: Min={MinReplicas}, Max={MaxReplicas}",
                resourcePath.ResourceId, scaleRequest.MinReplicas, scaleRequest.MaxReplicas);
            await Task.CompletedTask;
            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        private async Task<ResourceProviderActionResult> RestartAzureKubernetesServiceDeployment(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            // TODO: Implement actual Kubernetes deployment restart logic
            _logger.LogInformation("Restart action triggered for Kubernetes deployment {DeploymentName}", resourcePath.ResourceId);
            await Task.CompletedTask;
            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        private async Task<ResourceProviderActionResult> ScaleAzureKubernetesServiceDeployment(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity)
        {
            var scaleRequest = JsonSerializer.Deserialize<InfrastructureScaleRequest>(serializedAction)
                ?? throw new ResourceProviderException("The scale request is invalid.",
                    StatusCodes.Status400BadRequest);

            // TODO: Implement actual Kubernetes deployment scale logic
            _logger.LogInformation("Scale action triggered for Kubernetes deployment {DeploymentName} with replicas: {Replicas}",
                resourcePath.ResourceId, scaleRequest.Replicas);
            await Task.CompletedTask;
            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case InfrastructureResourceTypeNames.AzureContainerApps:
                    await DeleteResource<AzureContainerApp>(resourcePath);
                    break;
                case InfrastructureResourceTypeNames.AzureKubernetesServiceDeployments:
                    await DeleteResource<AzureKubernetesServiceDeployment>(resourcePath);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
            await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
        }

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
