using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
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
        private readonly IResourceProviderService _configurationResourceProvider;
        private readonly IResourceProviderService _agentResourceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="cosmosDBService">The Azure Cosmos DB service that contains
        /// user profiles.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="UserProfileService"/> type name.</param>
        /// <param name="callContext">Contains contextual data for the calling service.</param>
        /// <param name="resourceProviderServices">The collection of <see cref="IResourceProviderService"/> services.</param>
        public UserProfileService(IAzureCosmosDBService cosmosDBService,
            ILogger<UserProfileService> logger,
            IOrchestrationContext callContext,
            IEnumerable<IResourceProviderService> resourceProviderServices)
        {
            _cosmosDBService = cosmosDBService;
            _logger = logger;
            _callContext = callContext;

            _agentResourceProvider = resourceProviderServices
                .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Agent)
                ?? throw new ResourceProviderException($"The resource provider service for '{ResourceProviderNames.FoundationaLLM_Agent}' is not registered.");
            _configurationResourceProvider = resourceProviderServices
                .SingleOrDefault(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Configuration)
                ?? throw new ResourceProviderException($"The resource provider service for '{ResourceProviderNames.FoundationaLLM_Configuration}' is not registered.");
        }

        /// <inheritdoc/>
        public async Task<UserProfile?> GetUserProfileAsync(
            string instanceId,
            List<ResourceProviderGetResult<AgentBase>>? agents = null) =>
            await GetUserProfileInternalAsync(
                _callContext.CurrentUserIdentity?.UPN ??
                throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving the user profile."),
                true,
                agents);

        /// <inheritdoc/>
        public async Task<UserProfile?> GetUserProfileForUserAsync(string instanceId, string upn) =>
            await GetUserProfileInternalAsync(upn);

        /// <inheritdoc/>
        public async Task UpsertUserProfileAsync(string instanceId, UserProfile userProfile)
        {
            // Ensure the user profile contains the user's UPN.
            if (string.IsNullOrWhiteSpace(userProfile.UPN)
                || !string.Equals(
                        userProfile.UPN,
                        _callContext.CurrentUserIdentity?.UPN,
                        StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpStatusCodeException("The UPN in the user profile is invalid.",
                    StatusCodes.Status400BadRequest);
            }

            await _cosmosDBService.UpsertUserProfileAsync(userProfile);
        }

        /// <inheritdoc/>
        public async Task AddAgentToUserProfileAsync(
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
        public async Task RemoveAgentFromUserProfileAsync(
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

        /// <inheritdoc/>
        public async Task<UserData?> GetUserDataAsync(string instanceId) =>
            await _cosmosDBService.GetUserDataAsync(
                _callContext.CurrentUserIdentity?.UPN ??
                    throw new InvalidOperationException("Failed to retrieve the identity of the signed in user when retrieving the user data."));

        /// <inheritdoc/>
        public async Task UpsertUserDataAsync(string instanceId, UserData userData)
        {
            // Ensure the user data contains the user's UPN.
            if (string.IsNullOrWhiteSpace(userData.UPN)
                || !string.Equals(
                        userData.UPN,
                        _callContext.CurrentUserIdentity?.UPN,
                        StringComparison.OrdinalIgnoreCase))
            {
                throw new HttpStatusCodeException("The UPN in the user data is invalid.",
                    StatusCodes.Status400BadRequest);
            }

            await _cosmosDBService.UpsertUserDataAsync(userData);
        }

        private async Task<UserProfile> GetUserProfileInternalAsync(
            string upn,
            bool updateAgents = false,
            List<ResourceProviderGetResult<AgentBase>>? agents = null)
        {
            var userProfile = await _cosmosDBService.GetUserProfileAsync(upn);
            if (!updateAgents)
                return userProfile;

            var userData = await _cosmosDBService.GetUserDataAsync(upn);

            if (userProfile.UpdatedOn == DateTimeOffset.MinValue)
            {
                // This is an older user profile that needs to be updated to the newer version.
                // Retrieve the list of agents assigned to the user and update the user profile.
                // Then, create a user data document for the user.

                _logger.LogInformation("Updating user profile to latest version and creating user data for user '{UPN}'.",
                    upn);

                agents ??= await _agentResourceProvider.GetResourcesAsync<AgentBase>(
                    _callContext.InstanceId!,
                    _callContext.CurrentUserIdentity!,
                    new ResourceProviderGetOptions
                    {
                        IncludeActions = false,
                        IncludeRoles = false
                    });

                userProfile.Agents = [.. agents
                    .Select(a => a.Resource.ObjectId!)];
                userProfile.UpdatedOn = DateTimeOffset.UtcNow;
                await _cosmosDBService.UpsertUserProfileAsync(userProfile);
            }

            agents ??= await _agentResourceProvider.GetResourcesAsync<AgentBase>(
                _callContext.InstanceId!,
                _callContext.CurrentUserIdentity!,
                new ResourceProviderGetOptions
                {
                    IncludeActions = false,
                    IncludeRoles = false
                });

            if (userData is null)
            {
                // The user alredy has the newwer version of the user profile, but does not have
                // a user data document. Create a new user data document with an empty allowed agents list.
                // it will be populated later when the user retrieves the list of agents.

                _logger.LogInformation("Creating new user data for user '{UPN}'.",
                    upn);

                userData = new UserData(upn)
                {
                    AllowedAgents = [.. agents.Select(a => a.Resource.ObjectId!)]
                };
                await _cosmosDBService.UpsertUserDataAsync(userData);
            }
            else
            {
                // All agents that are not in the loaded list of allowed agents are
                // agents for which the user got permissions to use since the last time
                // the list of agents was loaded. These agents will be automatically enabled for the user.

                var newAllowedAgents = agents
                    .Where(a => !userData.AllowedAgents.Contains(a.Resource.ObjectId!))
                    .ToList();
                var newAllowedAgentIds = newAllowedAgents
                    .Select(a => a.Resource.ObjectId!)
                    .ToList();

                if (newAllowedAgents.Count > 0)
                {
                    _logger.LogInformation("Adding {AgentCount} new allowed agents to user data and enabing them for user {UPN}. The new agents are: {AgentNames}.",
                        newAllowedAgents.Count,
                        upn,
                        string.Join(',', newAllowedAgents.Select(a => a.Resource.Name)));

                    userData.AllowedAgents.AddRange(newAllowedAgentIds);
                    await _cosmosDBService.UpsertUserDataAsync(userData);

                    userProfile.Agents.AddRange(newAllowedAgentIds);
                    await _cosmosDBService.UpsertUserProfileAsync(userProfile);
                }
            }

            return userProfile;
        }
    }
}
