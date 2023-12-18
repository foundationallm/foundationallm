using Asp.Versioning;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.Metadata;
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
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class UserProfilesController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ClientBrandingConfiguration _settings;

        /// <summary>
        /// Constructor for the UserProfiles Controller.
        /// </summary>
        /// <param name="userProfileService">The Core service provides methods for managing the user profile.</param>
        /// <param name="settings">The branding configuration for the client.</param>
        public UserProfilesController(IUserProfileService userProfileService,
            IOptions<ClientBrandingConfiguration> settings)
        {
            _userProfileService = userProfileService;
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
        public async Task<IEnumerable<Agent>> GetAgents()
        {
            var agents = new List<Agent>();
            var globalAgentsList = _settings.AllowAgentSelection;
            UserProfile? userProfile;
            try
            {
                userProfile = await _userProfileService.GetUserProfileAsync();
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                userProfile = null;
            }
            
            if (!string.IsNullOrWhiteSpace(globalAgentsList))
            {
                var globalAgents = globalAgentsList.Split(',');
                agents.AddRange(globalAgents.Select(globalAgent => new Agent {Name = globalAgent.Trim(), Private = false}));
            }

            if (userProfile?.PrivateAgents != null)
            {
                agents.AddRange(userProfile.PrivateAgents.Select(agent => new Agent { Name = agent.Name, Private = true}));
            }

            return agents;
        }
    }
}
