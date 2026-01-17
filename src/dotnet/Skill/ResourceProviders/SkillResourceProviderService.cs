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
using FoundationaLLM.Common.Models.ResourceProviders.Skill;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Skill.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Skill.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Skill resource provider for procedural memory.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    /// <param name="proxyMode">Indicates whether the resource provider is running in proxy mode.</param>
    public class SkillResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Skill)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILogger<SkillResourceProviderService> logger,
        bool proxyMode = false)
        : ResourceProviderServiceBase<SkillReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true,
            proxyMode: proxyMode)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            SkillResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Skill;

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
                SkillResourceTypeNames.Skills => await LoadResources<Common.Models.ResourceProviders.Skill.Skill>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
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
                SkillResourceTypeNames.Skills => await UpdateSkill(resourcePath, serializedResource!, userIdentity),
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
                SharedResourceTypeNames.Management => resourcePath.Action switch
                {
                    ResourceProviderActions.TriggerCommand => await ExecuteManagementAction(
                        resourcePath,
                        authorizationResult,
                        serializedAction),
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                SkillResourceTypeNames.Skills => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckSkillName(authorizationResult, serializedAction),
                    ResourceProviderActions.Purge => await PurgeResource<Common.Models.ResourceProviders.Skill.Skill>(resourcePath),
                    SkillResourceProviderActions.Search => await SearchSkills(serializedAction, userIdentity),
                    SkillResourceProviderActions.Approve => await ApproveSkill(resourcePath, userIdentity),
                    _ => throw new ResourceProviderException(
                            $"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case SkillResourceTypeNames.Skills:
                    await DeleteResource<Common.Models.ResourceProviders.Skill.Skill>(resourcePath);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
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

        /// <inheritdoc/>
        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource, UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            resource.GetType() switch
            {
                Type t when t == typeof(Common.Models.ResourceProviders.Skill.Skill) =>
                    ((await UpdateSkill(
                        resourcePath,
                        (resource as Common.Models.ResourceProviders.Skill.Skill)!,
                        userIdentity)).ToResourceProviderUpsertResult<Common.Models.ResourceProviders.Skill.Skill>() as TResult)!,
                _ => throw new ResourceProviderException($"The resource type {resource.GetType().Name} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #endregion

        #region Resource management

        private async Task<ResourceNameCheckResult> CheckSkillName(
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction)
        {
            if (!authorizationResult.Authorized
                && !authorizationResult.HasRequiredRole)
                throw new ResourceProviderException("Access is not authorized.", StatusCodes.Status403Forbidden);

            return await CheckResourceName<Common.Models.ResourceProviders.Skill.Skill>(
                JsonSerializer.Deserialize<ResourceName>(serializedAction)!);
        }

        private async Task<ResourceProviderUpsertResult> UpdateSkill(ResourcePath resourcePath, string serializedSkill, UnifiedUserIdentity userIdentity)
        {
            var skill = JsonSerializer.Deserialize<Common.Models.ResourceProviders.Skill.Skill>(serializedSkill)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            return await UpdateSkill(resourcePath, skill, userIdentity);
        }

        private async Task<ResourceProviderUpsertResult> UpdateSkill(
            ResourcePath resourcePath,
            Common.Models.ResourceProviders.Skill.Skill skill,
            UnifiedUserIdentity userIdentity)
        {
            var existingSkillReference = await _resourceReferenceStore!.GetResourceReference(skill.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != skill.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var skillReference = new SkillReference
            {
                Name = skill.Name!,
                Type = SkillTypes.Skill,
                Filename = $"/{_name}/{skill.Name}.json",
                Deleted = false
            };

            skill.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            UpdateBaseProperties(skill, userIdentity, isNew: existingSkillReference is null);
            if (existingSkillReference is null)
                await CreateResource<Common.Models.ResourceProviders.Skill.Skill>(skillReference, skill);
            else
                await SaveResource<Common.Models.ResourceProviders.Skill.Skill>(existingSkillReference, skill);

            return new ResourceProviderUpsertResult
            {
                ObjectId = skill!.ObjectId,
                ResourceExists = existingSkillReference is not null
            };
        }

        private async Task<List<SkillSearchResult>> SearchSkills(string serializedRequest, UnifiedUserIdentity userIdentity)
        {
            var searchRequest = JsonSerializer.Deserialize<SkillSearchRequest>(serializedRequest)
                ?? throw new ResourceProviderException("The search request is invalid.",
                    StatusCodes.Status400BadRequest);

            // TODO: Implement vector search for semantic skill matching.
            // For now, return an empty list as a placeholder.
            // In a full implementation, this would:
            // 1. Generate an embedding for the query
            // 2. Search a vector index for skills scoped to the agent-user combination
            // 3. Return matching skills sorted by similarity

            await Task.CompletedTask;
            return [];
        }

        private async Task<ResourceProviderActionResult> ApproveSkill(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            var skillId = resourcePath.ResourceId
                ?? throw new ResourceProviderException("Skill ID is required.", StatusCodes.Status400BadRequest);

            var skill = await LoadResource<Common.Models.ResourceProviders.Skill.Skill>(skillId);
            if (skill == null)
                throw new ResourceProviderException($"Skill '{skillId}' not found.", StatusCodes.Status404NotFound);

            if (skill.Status != SkillStatus.PendingApproval)
                throw new ResourceProviderException($"Skill '{skillId}' is not pending approval.", StatusCodes.Status400BadRequest);

            skill.Status = SkillStatus.Active;
            UpdateBaseProperties(skill, userIdentity, isNew: false);

            var skillReference = await _resourceReferenceStore!.GetResourceReference(skillId);
            if (skillReference != null)
            {
                await SaveResource<Common.Models.ResourceProviders.Skill.Skill>(skillReference, skill);
            }

            return new ResourceProviderActionResult
            {
                IsSuccessResult = true
            };
        }

        #endregion
    }
}
