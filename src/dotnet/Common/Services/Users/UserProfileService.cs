using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Services.Users
{
    /// <inheritdoc/>
    public class UserProfileService : IUserProfileService
    {
        private readonly IAzureCosmosDBService _cosmosDBService;
        private readonly ILogger<UserProfileService> _logger;
        private readonly IOrchestrationContext _callContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="cosmosDBService">The Azure Cosmos DB service that contains
        /// user profiles.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="UserProfileService"/> type name.</param>
        /// <param name="callContext">Contains contextual data for the calling service.</param>
        public UserProfileService(IAzureCosmosDBService cosmosDBService,
            ILogger<UserProfileService> logger,
            IOrchestrationContext callContext)
        {
            _cosmosDBService = cosmosDBService;
            _logger = logger;
            _callContext = callContext;
        }

        /// <inheritdoc/>
        public async Task<UserProfile?> GetUserProfileAsync(string instanceId) => await _cosmosDBService.GetUserProfileAsync(_callContext.CurrentUserIdentity?.UPN ??
            throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving the user profile."));

        /// <inheritdoc/>
        public async Task<UserProfile?> GetUserProfileForUserAsync(string instanceId, string upn) =>
            await _cosmosDBService.GetUserProfileAsync(upn);
        
        /// <inheritdoc/>
        public async Task UpsertUserProfileAsync(string instanceId, UserProfile userProfile)
        {
            // Ensure the user profile contains the user's UPN.
            if (string.IsNullOrEmpty(userProfile.UPN))
            {
                userProfile.UPN = _callContext.CurrentUserIdentity?.UPN
                    ?? throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving chat sessions.");
                userProfile.Id = userProfile.UPN;
            }
            await _cosmosDBService.UpsertUserProfileAsync(userProfile);
        }

        /// <inheritdoc/>
        public async Task AddAgent(
            string instanceId,
            string agentObjectId)
        {
            if (string.IsNullOrWhiteSpace(agentObjectId))
                throw new HttpStatusCodeException("The agent object identifier is invalid.",
                    StatusCodes.Status400BadRequest);

            var resourcePath = ResourcePath.GetResourcePath(agentObjectId);

            if (resourcePath.InstanceId is null
                || resourcePath.InstanceId != instanceId
                || resourcePath.ResourceProvider is null
                || resourcePath.ResourceProvider != ResourceProviderNames.FoundationaLLM_Agent
                || resourcePath.ResourceTypeInstances.Count != 1
                || resourcePath.MainResourceTypeName != AgentResourceTypeNames.Agents
                || !resourcePath.HasResourceId
                || resourcePath.HasAction)
                throw new ResourceProviderException("The agent object identifier is invalid.",
                    StatusCodes.Status400BadRequest);

            var userProfile = await _cosmosDBService.GetUserProfileAsync(_callContext.CurrentUserIdentity?.UPN ??
                throw new HttpStatusCodeException("Failed to retrieve the identity of the signed in user when retrieving the user profile.",
                    StatusCodes.Status400BadRequest)
                ?? throw new HttpStatusCodeException("Could not find the user profile for the signed in user.",
                    StatusCodes.Status404NotFound));

            if (userProfile.Agents.Contains(agentObjectId))
                return;

            userProfile.Agents.Add(agentObjectId);

            await _cosmosDBService.PatchItemPropertiesAsync<UserProfile>(
                        AzureCosmosDBContainers.UserProfiles,
                        _callContext.CurrentUserIdentity.UPN,
                        _callContext.CurrentUserIdentity.UPN,
                        _callContext.CurrentUserIdentity.UPN,
                        new Dictionary<string, object?>()
                        {
                            { "/agents", userProfile.Agents }
                        },
                        default);
            return;
        }

        /// <inheritdoc/>
        public async Task RemoveAgent(
            string instanceId,
            string agentObjectId)
        {
            if (string.IsNullOrWhiteSpace(agentObjectId))
                throw new HttpStatusCodeException("The agent object identifier is invalid.",
                    StatusCodes.Status400BadRequest);

            var resourcePath = ResourcePath.GetResourcePath(agentObjectId);

            if (resourcePath.InstanceId is null
                || resourcePath.InstanceId != instanceId
                || resourcePath.ResourceProvider is null
                || resourcePath.ResourceProvider != ResourceProviderNames.FoundationaLLM_Agent
                || resourcePath.ResourceTypeInstances.Count != 1
                || resourcePath.MainResourceTypeName != AgentResourceTypeNames.Agents
                || !resourcePath.HasResourceId
                || resourcePath.HasAction)
                throw new ResourceProviderException("The agent object identifier is invalid.",
                    StatusCodes.Status400BadRequest);

            var userProfile = await _cosmosDBService.GetUserProfileAsync(_callContext.CurrentUserIdentity?.UPN ??
                throw new HttpStatusCodeException("Failed to retrieve the identity of the signed in user when retrieving the user profile.",
                    StatusCodes.Status400BadRequest)
                ?? throw new HttpStatusCodeException("Could not find the user profile for the signed in user.",
                    StatusCodes.Status404NotFound));

            if (!userProfile.Agents.Contains(agentObjectId))
                return;

            userProfile.Agents.Remove(agentObjectId);

            await _cosmosDBService.PatchItemPropertiesAsync<UserProfile>(
                        AzureCosmosDBContainers.UserProfiles,
                        _callContext.CurrentUserIdentity.UPN,
                        _callContext.CurrentUserIdentity.UPN,
                        _callContext.CurrentUserIdentity.UPN,
                        new Dictionary<string, object?>()
                        {
                            { "/agents", userProfile.Agents }
                        },
                        default);
            return;
        }
    }
}
