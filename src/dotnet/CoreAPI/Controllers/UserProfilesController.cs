using FoundationaLLM.Core.Interfaces;
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
    /// <param name="userProfileService">The Core service provides methods for managing the user profile.</param>
    [Authorize(Policy = "DefaultPolicy")]
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
    }
}
