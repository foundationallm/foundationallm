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
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Plugin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Vector.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Vector resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>    
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The factory responsible for creating loggers.</param>
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class VectorResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vector_Storage)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory,
        bool proxyMode = false)
        : ResourceProviderServiceBase<VectorReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<VectorResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            VectorResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Vector;

        /// <inheritdoc/>
        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected async override Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                VectorResourceTypeNames.VectorDatabases => await LoadResources<VectorDatabase>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected async override Task<object> UpsertResourceAsync(
            ResourcePath resourcePath,
            string? serializedResource,
            ResourceProviderFormFile? formFile,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            Func<object, bool>? requestPayloadValidator = null) =>

            resourcePath.ResourceTypeName switch
            {
                VectorResourceTypeNames.VectorDatabases => await UpdateVectorDatabase(
                    resourcePath,
                    serializedResource!,
                    authorizationResult,
                    userIdentity),
                _ => throw new ResourceProviderException(
                    $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
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
                VectorResourceTypeNames.VectorDatabases => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckVectorDatabaseName(
                        authorizationResult,
                        serializedAction),
                    _ => throw new ResourceProviderException(
                        $"The action {resourcePath.Action} is not supported for resource type {resourcePath.ResourceTypeName} by the {_name} resource provider.",
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
                case VectorResourceTypeNames.VectorDatabases:
                    await DeleteResource<VectorDatabase>(resourcePath);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            }
            ;
            await SendResourceProviderEvent(EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand);
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

        #region Resource management

        private async Task<ResourceNameCheckResult> CheckVectorDatabaseName(
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction)
        {
            if (!authorizationResult.Authorized
                && !authorizationResult.HasRequiredRole)
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);

            return await CheckResourceName<VectorDatabase>(
                JsonSerializer.Deserialize<ResourceName>(serializedAction)!);
        }

        private async Task<ResourceProviderUpsertResult> UpdateVectorDatabase(
            ResourcePath resourcePath,
            string serializedVectorDatabase,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity)
        {
            var vectorDatabase = JsonSerializer.Deserialize<VectorDatabase>(serializedVectorDatabase)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var existingVectorDatabaseReference = await _resourceReferenceStore!.GetResourceReference(vectorDatabase.Name);

            if (existingVectorDatabaseReference is not null
                && !authorizationResult.Authorized)
            {
                // The resource already exists and the user is not authorized to update it.
                // Irrespective of whether the user has the required role or not, we need to throw an exception in the case of existing resources.
                // The required role only allows the user to create a new resource.
                // This check is needed because it's only here that we can determine if the resource exists.
                _logger.LogWarning("Access to the resource path {ResourcePath} was not authorized for user {UserName} : userId {UserId}.",
                    resourcePath.RawResourcePath, userIdentity!.Username, userIdentity!.UserId);
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);
            }

            if (resourcePath.ResourceTypeInstances[0].ResourceId != vectorDatabase.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var vectorDatabaseReference = new VectorReference
            {
                Name = vectorDatabase.Name!,
                Type = vectorDatabase.Type!,
                Filename = $"/{_name}/{vectorDatabase.Name}.json",
                Deleted = false
            };

            if (string.IsNullOrWhiteSpace(vectorDatabase.ObjectId))
                vectorDatabase.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            await _validator.ValidateAndThrowAsync<VectorDatabase>(vectorDatabase);

            UpdateBaseProperties(vectorDatabase, userIdentity, isNew: (existingVectorDatabaseReference is null));

            if (existingVectorDatabaseReference is null)
                await CreateResource<VectorDatabase>(vectorDatabaseReference, vectorDatabase);
            else
                await SaveResource<VectorDatabase>(existingVectorDatabaseReference, vectorDatabase);

            return new ResourceProviderUpsertResult
            {
                ObjectId = vectorDatabase!.ObjectId,
                ResourceExists = existingVectorDatabaseReference is not null
            };
        }

        #endregion
    }
}
