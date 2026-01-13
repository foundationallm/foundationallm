using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Quota;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Quota.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Quota.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Quota resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="quotaStorageService">The <see cref="IStorageService"/> providing storage services for the quota-store.json file.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class QuotaResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt)] IStorageService storageService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Quota)] IStorageService quotaStorageService,
        IQuotaService quotaService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<QuotaResourceProviderService> logger,
        bool proxyMode = false)
        : ResourceProviderServiceBase<QuotaReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            [],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        private const string QUOTA_STORAGE_CONTAINER_NAME = "quota";
        private const string QUOTA_STORE_FILE_PATH = "/quota-store.json";
        private readonly IStorageService _quotaStorageService = quotaStorageService;
        private readonly IQuotaService _quotaService = quotaService;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            QuotaResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Quota;

        /// <inheritdoc/>
        /// <remarks>
        /// This override bypasses authorization for the Quota resource provider during development.
        /// In production, proper role assignments should be configured.
        /// </remarks>
        protected override bool BypassResourceAuthorization => true;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            // Synchronize the quota-store.json file on initialization
            await SyncQuotaStoreFile();
        }

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.MainResourceTypeName switch
            {
                QuotaResourceTypeNames.QuotaDefinitions => await LoadResources<QuotaDefinition>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                QuotaResourceTypeNames.QuotaMetrics => await _quotaService.GetQuotaUsageMetricsAsync(),
                QuotaResourceTypeNames.QuotaEvents => await _quotaService.GetQuotaEventsAsync(new QuotaEventFilter()),
                _ => throw new ResourceProviderException(
                    $"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
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
                QuotaResourceTypeNames.QuotaDefinitions => await UpdateQuotaDefinition(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException(
                        $"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest),
            };

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>
            resourcePath.ResourceTypeName switch
            {
                QuotaResourceTypeNames.QuotaDefinitions => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckQuotaName(authorizationResult, serializedAction),
                    ResourceProviderActions.Purge => await PurgeResource<QuotaDefinition>(resourcePath),
                    _ => throw new ResourceProviderException(
                            $"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
                },
                QuotaResourceTypeNames.QuotaMetrics => resourcePath.Action switch
                {
                    ResourceProviderActions.Filter => await GetFilteredQuotaMetrics(serializedAction),
                    _ => throw new ResourceProviderException(
                            $"The action {resourcePath.Action} is not supported for {QuotaResourceTypeNames.QuotaMetrics} by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
                },
                QuotaResourceTypeNames.QuotaEvents => resourcePath.Action switch
                {
                    ResourceProviderActions.Filter => await GetFilteredQuotaEvents(serializedAction),
                    "summary" => await GetQuotaEventSummary(serializedAction),
                    _ => throw new ResourceProviderException(
                            $"The action {resourcePath.Action} is not supported for {QuotaResourceTypeNames.QuotaEvents} by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException(
                    $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case QuotaResourceTypeNames.QuotaDefinitions:
                    await DeleteResource<QuotaDefinition>(resourcePath);
                    // Sync the quota-store.json file after deletion
                    await SyncQuotaStoreFile();
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
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

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource, UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            resource.GetType() switch
            {
                Type t when t == typeof(QuotaDefinition) =>
                    ((await UpdateQuotaDefinition(
                        resourcePath,
                        (resource as QuotaDefinition)!,
                        userIdentity)).ToResourceProviderUpsertResult<QuotaDefinition>() as TResult)!,
                _ => throw new ResourceProviderException($"The resource type {resource.GetType().Name} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #endregion

        #region Quota metrics operations

        private async Task<List<QuotaUsageMetrics>> GetFilteredQuotaMetrics(string serializedFilter)
        {
            var filter = JsonSerializer.Deserialize<QuotaMetricsFilter>(serializedFilter)
                ?? new QuotaMetricsFilter();
            
            return await _quotaService.GetQuotaUsageMetricsAsync(filter);
        }

        #endregion

        #region Quota events operations

        private async Task<List<QuotaEventDocument>> GetFilteredQuotaEvents(string serializedFilter)
        {
            var filter = JsonSerializer.Deserialize<QuotaEventFilter>(serializedFilter)
                ?? throw new ResourceProviderException("The filter object is invalid.", StatusCodes.Status400BadRequest);
            
            return await _quotaService.GetQuotaEventsAsync(filter);
        }

        private async Task<List<QuotaEventSummary>> GetQuotaEventSummary(string serializedRequest)
        {
            var request = JsonSerializer.Deserialize<QuotaEventSummaryRequest>(serializedRequest)
                ?? throw new ResourceProviderException("The request object is invalid.", StatusCodes.Status400BadRequest);
            
            return await _quotaService.GetQuotaEventSummaryAsync(request.QuotaName, request.StartTime, request.EndTime);
        }

        #endregion

        #region Resource management

        private async Task<ResourceNameCheckResult> CheckQuotaName(
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction)
        {
            // Note: CheckName is a read-only operation that doesn't expose sensitive data,
            // so we allow it to proceed without strict authorization.
            // The actual create/update operation will enforce proper authorization.
            return await CheckResourceName<QuotaDefinition>(
                JsonSerializer.Deserialize<ResourceName>(serializedAction)!);
        }

        private async Task<ResourceProviderUpsertResult> UpdateQuotaDefinition(ResourcePath resourcePath, string serializedQuota, UnifiedUserIdentity userIdentity)
        {
            var quota = JsonSerializer.Deserialize<QuotaDefinition>(serializedQuota)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            return await UpdateQuotaDefinition(resourcePath, quota, userIdentity);
        }

        private async Task<ResourceProviderUpsertResult> UpdateQuotaDefinition(
            ResourcePath resourcePath,
            QuotaDefinition quota,
            UnifiedUserIdentity userIdentity)
        {
            var existingQuotaReference = await _resourceReferenceStore!.GetResourceReference(quota.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != quota.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var quotaReference = new QuotaReference
            {
                Name = quota.Name!,
                Type = quota.Type ?? "quota-definition",
                Filename = $"/{_name}/{quota.Name}.json",
                Deleted = false
            };

            quota.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            UpdateBaseProperties(quota, userIdentity, isNew: existingQuotaReference is null);
            if (existingQuotaReference is null)
                await CreateResource<QuotaDefinition>(quotaReference, quota);
            else
                await SaveResource<QuotaDefinition>(existingQuotaReference, quota);

            // Sync the quota-store.json file after create/update
            await SyncQuotaStoreFile();

            return new ResourceProviderUpsertResult
            {
                ObjectId = quota!.ObjectId,
                ResourceExists = existingQuotaReference is not null
            };
        }

        /// <summary>
        /// Rebuilds the quota-store.json file from all quota definitions in the resource provider.
        /// This ensures the Core API's QuotaService has access to the latest quota definitions.
        /// </summary>
        private async Task SyncQuotaStoreFile()
        {
            try
            {
                // Load all quota definitions from the resource provider storage
                var allReferences = await _resourceReferenceStore!.GetAllResourceReferences<QuotaDefinition>();
                var quotaDefinitions = new List<QuotaDefinition>();

                foreach (var reference in allReferences.Where(r => !r.Deleted))
                {
                    var quota = await LoadResource<QuotaDefinition>(reference.Name);
                    if (quota != null && !quota.Deleted)
                    {
                        quotaDefinitions.Add(quota);
                    }
                }

                // Serialize the quota definitions to JSON
                var json = JsonSerializer.Serialize(quotaDefinitions, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                // Write to quota-store.json
                await _quotaStorageService.WriteFileAsync(
                    QUOTA_STORAGE_CONTAINER_NAME,
                    QUOTA_STORE_FILE_PATH,
                    json,
                    default,
                    default);

                _logger.LogInformation("Successfully synchronized quota-store.json with {QuotaCount} quota definitions.",
                    quotaDefinitions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to synchronize quota-store.json file.");
                throw;
            }
        }

        #endregion
    }
}
