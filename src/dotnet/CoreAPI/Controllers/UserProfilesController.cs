using Asp.Versioning;
using FoundationaLLM.Agent.Constants;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;


namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing user profiles.
    /// </summary>
    [Authorize]
    [Authorize(Policy = "RequiredScope")]
    [ApiController]
    [Route("[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ClientBrandingConfiguration _settings;
        private readonly IResourceProviderService _agentResourceProvider;

        /// <summary>
        /// Constructor for the UserProfiles Controller.
        /// </summary>
        /// <param name="userProfileService">The Core service provides methods for managing the user profile.</param>
        /// <param name="settings">The branding configuration for the client.</param>
        /// <param name="resourceProviderServices">The list of <see cref="IResourceProviderService"/> resource provider services.</param>
        public UserProfilesController(
            IUserProfileService userProfileService,
            IOptions<ClientBrandingConfiguration> settings,
            IEnumerable<IResourceProviderService> resourceProviderServices)
        {
            _userProfileService = userProfileService;
            var resourceProviderServicesDictionary = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
            if (!resourceProviderServicesDictionary.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
            _agentResourceProvider = agentResourceProvider;
            _settings = settings.Value;
        }

        /// <summary>
        /// Retrieves the branding information for the client.
        /// </summary>
        [HttpGet(Name = "GetUserProfile")]
        public async Task<IActionResult> Index() =>
            Ok(await _userProfileService.GetUserProfileAsync());

        /// <summary>
        /// Retrieves a list of global and private agents.
        /// </summary>
        /// <returns></returns>
        [HttpGet("agents", Name = "GetAgents")]
        public async Task<IEnumerable<AgentHint>> GetAgents()
        {
            var agents = new List<AgentHint>();
            
            var legacyAgentsList = _settings.AllowAgentSelection;
            UserProfile? userProfile;

            try
            {
                userProfile = await _userProfileService.GetUserProfileAsync();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                userProfile = null;
            }
            
            if (await _agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}") is List<AgentBase> globalAgentsList && globalAgentsList.Count != 0)
            {
                agents.AddRange(globalAgentsList.Select(globalAgent => new AgentHint { Name = globalAgent.Name, Private = false}));
            }

            if (!string.IsNullOrWhiteSpace(legacyAgentsList))
            {
                var legacyAgents = legacyAgentsList.Split(',');
                // Only add the agent if it does not already exist in the list of global agents.
                foreach (var legacyAgent in legacyAgents)
                {
                    if (agents.All(agent => agent.Name != legacyAgent.Trim()))
                    {
                        agents.Add(new AgentHint { Name = legacyAgent.Trim(), Private = false });
                    }
                }
            }

            if (userProfile?.PrivateAgents != null)
            {
                agents.AddRange(userProfile.PrivateAgents.Select(agent => new AgentHint { Name = agent.Name, Private = true}));
            }

            return agents;
        }
    }
}
