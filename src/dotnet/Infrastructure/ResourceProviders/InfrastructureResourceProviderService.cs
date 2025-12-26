using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager.ContainerService;
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
using k8s;
using k8s.Models;
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
        private readonly TokenCredential _credential = new DefaultAzureCredential();

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

            // Update Azure Container App using Azure SDK
            if (!string.IsNullOrEmpty(containerApp.AzureResourceId))
            {
                await UpdateAzureContainerAppInAzure(containerApp);
            }

            var reference = new InfrastructureReference
            {
                Name = containerApp.Name!,
                Type = InfrastructureTypes.AzureContainerApp,
                Filename = $"/{_name}/{resourcePath.MainResourceId}/{containerApp.Name}.json",
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

        private async Task UpdateAzureContainerAppInAzure(AzureContainerApp containerApp)
        {
            try
            {
                var armClient = new ArmClient(_credential);
                var containerAppResourceId = new ResourceIdentifier(containerApp.AzureResourceId!);
                var containerAppResource = armClient.GetContainerAppResource(containerAppResourceId);

                var response = await containerAppResource.GetAsync();
                var existingApp = response.Value;
                var appData = existingApp.Data;

                // Update scale settings if provided
                if (containerApp.MinReplicas.HasValue || containerApp.MaxReplicas.HasValue)
                {
                    if (appData.Template != null)
                    {
                        if (appData.Template.Scale != null)
                        {
                            appData.Template.Scale.MinReplicas = containerApp.MinReplicas ?? appData.Template.Scale.MinReplicas;
                            appData.Template.Scale.MaxReplicas = containerApp.MaxReplicas ?? appData.Template.Scale.MaxReplicas;
                        }
                        else
                        {
                            appData.Template.Scale = new ContainerAppScale
                            {
                                MinReplicas = containerApp.MinReplicas,
                                MaxReplicas = containerApp.MaxReplicas
                            };
                        }
                    }
                    await containerAppResource.UpdateAsync(Azure.WaitUntil.Completed, appData);
                }

                _logger.LogInformation("Successfully updated Azure Container App {ContainerAppName} in Azure", containerApp.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Azure Container App {ContainerAppName} in Azure", containerApp.Name);
                throw new ResourceProviderException($"Failed to update Azure Container App {containerApp.Name}: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
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

            // Update Kubernetes deployment using Kubernetes client
            if (!string.IsNullOrEmpty(deployment.ClusterObjectId) && deployment.Replicas.HasValue)
            {
                await UpdateKubernetesDeploymentInCluster(deployment);
            }

            var reference = new InfrastructureReference
            {
                Name = deployment.Name!,
                Type = InfrastructureTypes.AzureKubernetesServiceDeployment,
                Filename = $"/{_name}/{resourcePath.MainResourceId}/{deployment.Name}.json",
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

        private async Task UpdateKubernetesDeploymentInCluster(AzureKubernetesServiceDeployment deployment)
        {
            try
            {
                if (string.IsNullOrEmpty(deployment.ClusterObjectId))
                {
                    throw new ResourceProviderException("ClusterObjectId is required for Kubernetes deployment operations.",
                        StatusCodes.Status400BadRequest);
                }

                var kubeConfig = await GetKubernetesConfigFromCluster(deployment.ClusterObjectId);
                using var client = new Kubernetes(kubeConfig);

                var k8sNamespace = deployment.Namespace ?? "default";
                var existingDeployment = await client.ReadNamespacedDeploymentAsync(deployment.Name!, k8sNamespace);

                if (deployment.Replicas.HasValue && existingDeployment?.Spec != null)
                {
                    existingDeployment.Spec.Replicas = deployment.Replicas;
                    await client.ReplaceNamespacedDeploymentAsync(existingDeployment, deployment.Name!, k8sNamespace);
                }

                _logger.LogInformation("Successfully updated Kubernetes deployment {DeploymentName} in cluster", deployment.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Kubernetes deployment {DeploymentName}", deployment.Name);
                throw new ResourceProviderException($"Failed to update Kubernetes deployment {deployment.Name}: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
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
            _logger.LogInformation("Restart action triggered for Azure Container App {ContainerAppName}", resourcePath.ResourceId);

            try
            {
                var containerApp = await LoadResource<AzureContainerApp>(resourcePath.ResourceId!);
                if (containerApp != null && !string.IsNullOrEmpty(containerApp.AzureResourceId))
                {
                    var armClient = new ArmClient(_credential);
                    var containerAppResourceId = new ResourceIdentifier(containerApp.AzureResourceId);
                    var containerAppResource = armClient.GetContainerAppResource(containerAppResourceId);

                    // Get the current revision and restart by creating a new revision
                    var response = await containerAppResource.GetAsync();
                    var existingApp = response.Value;
                    var appData = existingApp.Data;

                    // To restart a Container App, we update it with a new revision suffix
                    if (appData.Template != null)
                    {
                        appData.Template.RevisionSuffix = $"restart-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                    }

                    await containerAppResource.UpdateAsync(Azure.WaitUntil.Completed, appData);
                    _logger.LogInformation("Successfully restarted Azure Container App {ContainerAppName}", resourcePath.ResourceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart Azure Container App {ContainerAppName}", resourcePath.ResourceId);
                throw new ResourceProviderException($"Failed to restart Azure Container App {resourcePath.ResourceId}: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }

            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        private async Task<ResourceProviderActionResult> ScaleAzureContainerApp(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity)
        {
            var scaleRequest = JsonSerializer.Deserialize<InfrastructureScaleRequest>(serializedAction)
                ?? throw new ResourceProviderException("The scale request is invalid.",
                    StatusCodes.Status400BadRequest);

            _logger.LogInformation("Scale action triggered for Azure Container App {ContainerAppName} with replicas: Replicas={Replicas}, Min={MinReplicas}, Max={MaxReplicas}",
                resourcePath.ResourceId, scaleRequest.Replicas, scaleRequest.MinReplicas, scaleRequest.MaxReplicas);

            try
            {
                var containerApp = await LoadResource<AzureContainerApp>(resourcePath.ResourceId!);
                if (containerApp != null && !string.IsNullOrEmpty(containerApp.AzureResourceId))
                {
                    var armClient = new ArmClient(_credential);
                    var containerAppResourceId = new ResourceIdentifier(containerApp.AzureResourceId);
                    var containerAppResource = armClient.GetContainerAppResource(containerAppResourceId);

                    var response = await containerAppResource.GetAsync();
                    var existingApp = response.Value;
                    var appData = existingApp.Data;

                    if (appData.Template?.Scale != null)
                    {
                        appData.Template.Scale.MinReplicas = scaleRequest.MinReplicas;
                        appData.Template.Scale.MaxReplicas = scaleRequest.MaxReplicas;
                    }
                    else if (appData.Template != null)
                    {
                        appData.Template.Scale = new ContainerAppScale
                        {
                            MinReplicas = scaleRequest.MinReplicas,
                            MaxReplicas = scaleRequest.MaxReplicas
                        };
                    }

                    await containerAppResource.UpdateAsync(Azure.WaitUntil.Completed, appData);
                    _logger.LogInformation("Successfully scaled Azure Container App {ContainerAppName}", resourcePath.ResourceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scale Azure Container App {ContainerAppName}", resourcePath.ResourceId);
                throw new ResourceProviderException($"Failed to scale Azure Container App {resourcePath.ResourceId}: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }

            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        private async Task<ResourceProviderActionResult> RestartAzureKubernetesServiceDeployment(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            _logger.LogInformation("Restart action triggered for Kubernetes deployment {DeploymentName}", resourcePath.ResourceId);

            try
            {
                var deployment = await LoadResource<AzureKubernetesServiceDeployment>(resourcePath.ResourceId!);
                if (deployment != null && !string.IsNullOrEmpty(deployment.ClusterObjectId))
                {
                    var kubeConfig = await GetKubernetesConfigFromCluster(deployment.ClusterObjectId);
                    using var client = new Kubernetes(kubeConfig);

                    var k8sNamespace = deployment.Namespace ?? "default";
                    var existingDeployment = await client.ReadNamespacedDeploymentAsync(deployment.Name!, k8sNamespace);

                    if (existingDeployment?.Spec?.Template?.Metadata != null)
                    {
                        // Add or update restart annotation to trigger rolling restart
                        existingDeployment.Spec.Template.Metadata.Annotations ??= new Dictionary<string, string>();
                        existingDeployment.Spec.Template.Metadata.Annotations["kubectl.kubernetes.io/restartedAt"] =
                            DateTimeOffset.UtcNow.ToString("o");

                        await client.ReplaceNamespacedDeploymentAsync(existingDeployment, deployment.Name!, k8sNamespace);
                        _logger.LogInformation("Successfully restarted Kubernetes deployment {DeploymentName}", resourcePath.ResourceId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart Kubernetes deployment {DeploymentName}", resourcePath.ResourceId);
                throw new ResourceProviderException($"Failed to restart Kubernetes deployment {resourcePath.ResourceId}: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }

            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        private async Task<ResourceProviderActionResult> ScaleAzureKubernetesServiceDeployment(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity)
        {
            var scaleRequest = JsonSerializer.Deserialize<InfrastructureScaleRequest>(serializedAction)
                ?? throw new ResourceProviderException("The scale request is invalid.",
                    StatusCodes.Status400BadRequest);

            _logger.LogInformation("Scale action triggered for Kubernetes deployment {DeploymentName} with replicas: Replicas={Replicas}, Min={MinReplicas}, Max={MaxReplicas}",
                resourcePath.ResourceId, scaleRequest.Replicas, scaleRequest.MinReplicas, scaleRequest.MaxReplicas);

            try
            {
                var deployment = await LoadResource<AzureKubernetesServiceDeployment>(resourcePath.ResourceId!);
                if (deployment != null && !string.IsNullOrEmpty(deployment.ClusterObjectId) && scaleRequest.Replicas.HasValue)
                {
                    var kubeConfig = await GetKubernetesConfigFromCluster(deployment.ClusterObjectId);
                    using var client = new Kubernetes(kubeConfig);

                    var k8sNamespace = deployment.Namespace ?? "default";
                    var existingDeployment = await client.ReadNamespacedDeploymentAsync(deployment.Name!, k8sNamespace);

                    if (existingDeployment?.Spec != null)
                    {
                        existingDeployment.Spec.Replicas = scaleRequest.Replicas;
                        await client.ReplaceNamespacedDeploymentAsync(existingDeployment, deployment.Name!, k8sNamespace);
                        _logger.LogInformation("Successfully scaled Kubernetes deployment {DeploymentName} to {Replicas} replicas",
                            resourcePath.ResourceId, scaleRequest.Replicas);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to scale Kubernetes deployment {DeploymentName}", resourcePath.ResourceId);
                throw new ResourceProviderException($"Failed to scale Kubernetes deployment {resourcePath.ResourceId}: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }

            return new ResourceProviderActionResult(resourcePath.RawResourcePath, true);
        }

        private async Task<KubernetesClientConfiguration> GetKubernetesConfigFromCluster(string clusterObjectId)
        {
            try
            {
                // Load the AKS cluster resource to get credentials
                var aksCluster = await LoadResource<AzureKubernetesService>(clusterObjectId);
                if (aksCluster == null || string.IsNullOrEmpty(aksCluster.AzureResourceId))
                {
                    throw new ResourceProviderException($"AKS cluster {clusterObjectId} not found or missing Azure resource ID.",
                        StatusCodes.Status404NotFound);
                }

                var armClient = new ArmClient(_credential);
                var aksResourceId = new ResourceIdentifier(aksCluster.AzureResourceId);
                var aksResource = armClient.GetContainerServiceManagedClusterResource(aksResourceId);

                // Get admin credentials
                var credentialsResponse = await aksResource.GetClusterAdminCredentialsAsync();
                var credentials = credentialsResponse.Value;

                if (credentials.Kubeconfigs == null || credentials.Kubeconfigs.Count == 0)
                {
                    throw new ResourceProviderException($"No kubeconfig available for AKS cluster {clusterObjectId}.",
                        StatusCodes.Status500InternalServerError);
                }

                var kubeConfigBytes = credentials.Kubeconfigs[0].Value;
                if (kubeConfigBytes == null || kubeConfigBytes.Length == 0)
                {
                    throw new ResourceProviderException($"Invalid kubeconfig data for AKS cluster {clusterObjectId}.",
                        StatusCodes.Status500InternalServerError);
                }

                using var stream = new MemoryStream(kubeConfigBytes);
                var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(stream);

                if (string.IsNullOrEmpty(config.Host))
                {
                    throw new ResourceProviderException($"Invalid kubeconfig: missing host configuration for AKS cluster {clusterObjectId}.",
                        StatusCodes.Status500InternalServerError);
                }

                return config;
            }
            catch (ResourceProviderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Kubernetes config for cluster {ClusterObjectId}", clusterObjectId);
                throw new ResourceProviderException($"Failed to get Kubernetes config for cluster: {ex.Message}",
                    StatusCodes.Status500InternalServerError);
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case InfrastructureResourceTypeNames.AzureContainerApps:
                    await DeleteAzureContainerApp(resourcePath);
                    break;
                case InfrastructureResourceTypeNames.AzureKubernetesServiceDeployments:
                    await DeleteAzureKubernetesServiceDeployment(resourcePath);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
            await SendResourceProviderEvent(
                    EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
        }

        private async Task DeleteAzureContainerApp(ResourcePath resourcePath)
        {
            var containerApp = await LoadResource<AzureContainerApp>(resourcePath.ResourceId!);
            if (containerApp != null && !string.IsNullOrEmpty(containerApp.AzureResourceId))
            {
                try
                {
                    var armClient = new ArmClient(_credential);
                    var containerAppResourceId = new ResourceIdentifier(containerApp.AzureResourceId);
                    var containerAppResource = armClient.GetContainerAppResource(containerAppResourceId);
                    await containerAppResource.DeleteAsync(Azure.WaitUntil.Completed);
                    _logger.LogInformation("Successfully deleted Azure Container App {ContainerAppName} from Azure", resourcePath.ResourceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete Azure Container App {ContainerAppName} from Azure", resourcePath.ResourceId);
                    throw new ResourceProviderException($"Failed to delete Azure Container App {resourcePath.ResourceId}: {ex.Message}",
                        StatusCodes.Status500InternalServerError);
                }
            }
            await DeleteResource<AzureContainerApp>(resourcePath);
        }

        private async Task DeleteAzureKubernetesServiceDeployment(ResourcePath resourcePath)
        {
            var deployment = await LoadResource<AzureKubernetesServiceDeployment>(resourcePath.ResourceId!);
            if (deployment != null && !string.IsNullOrEmpty(deployment.ClusterObjectId))
            {
                try
                {
                    var kubeConfig = await GetKubernetesConfigFromCluster(deployment.ClusterObjectId);
                    using var client = new Kubernetes(kubeConfig);

                    var k8sNamespace = deployment.Namespace ?? "default";
                    await client.DeleteNamespacedDeploymentAsync(deployment.Name!, k8sNamespace);
                    _logger.LogInformation("Successfully deleted Kubernetes deployment {DeploymentName} from cluster", resourcePath.ResourceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete Kubernetes deployment {DeploymentName} from cluster", resourcePath.ResourceId);
                    throw new ResourceProviderException($"Failed to delete Kubernetes deployment {resourcePath.ResourceId}: {ex.Message}",
                        StatusCodes.Status500InternalServerError);
                }
            }
            await DeleteResource<AzureKubernetesServiceDeployment>(resourcePath);
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
