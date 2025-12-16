using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing user profiles.
    /// </summary>
    /// <remarks>
    /// Constructor for the UserProfiles Controller.
    /// </remarks>
    /// <param name="userProfileService">Service that provides methods for managing the user profile.</param>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class UserProfilesController(
        IUserProfileService userProfileService) : ControllerBase
    {
        private readonly IUserProfileService _userProfileService = userProfileService;

        /// <summary>
        /// Retrieves user profiles.
        /// </summary>
        /// <param name="instanceId">The instance identifier.</param>
        [HttpGet(Name = "GetUserProfile")]
        public async Task<IActionResult> Index(string instanceId) =>
            Ok(await _userProfileService.GetUserProfileAsync(instanceId));

        /// <summary>
        /// Adds an agent to the user's profile.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="updateRequest">The user profile update request.</param>
        /// <returns></returns>
        [HttpPost("add-agent")]
        public async Task<IActionResult> AddAgentToUserProfile(
            string instanceId,
            [FromBody] UserProfileUpdateRequest updateRequest)
        {
            await _userProfileService.AddAgentToUserProfileAsync(instanceId, updateRequest.AgentObjectId!);
            return Ok();
        }

        /// <summary>
        /// Adds an agent to the user's profile.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="updateRequest">The user profile update request.</param>
        /// <returns></returns>
        [HttpPost("remove-agent")]
        public async Task<IActionResult> RemoveAgentFromUserProfile(
            string instanceId,
            [FromBody] UserProfileUpdateRequest updateRequest)
        {
            await _userProfileService.RemoveAgentFromUserProfileAsync(instanceId, updateRequest.AgentObjectId!);
            return Ok();
        }
    }
}
