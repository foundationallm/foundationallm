using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for OneDrive integration.
    /// </summary>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class OneDriveWorkSchoolController : ControllerBase
    {
        private readonly IOrchestrationContext _callContext;
        private readonly IOneDriveWorkSchoolService _oneDriveWorkSchoolService;

        /// <summary>
        /// The controller for OneDrive integration.
        /// </summary>
        /// <param name="callContext">The <see cref="IOrchestrationContext"/> call context of the request being handled.</param>
        /// <param name="oneDriveWorkSchoolService">The <see cref="IOneDriveWorkSchoolService"/> OneDrive service.</param>
        /// <exception cref="ResourceProviderException"></exception>
        public OneDriveWorkSchoolController(
            IOrchestrationContext callContext,
            IOneDriveWorkSchoolService oneDriveWorkSchoolService)
        {
            _callContext = callContext;
            _oneDriveWorkSchoolService = oneDriveWorkSchoolService;
        }

        /// <summary>
        /// Connects to user's OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        [HttpPost("connect")]
        public async Task<IActionResult> Connect(string instanceId)
        {
            await _oneDriveWorkSchoolService.Connect(instanceId);

            return Ok();
        }

        /// <summary>
        /// Disconnect from user's OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect(string instanceId)
        {
            await _oneDriveWorkSchoolService.Disconnect(instanceId);

            return Ok();
        }

        /// <summary>
        /// Downloads a file from the user's connected OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="sessionId">The session ID from which the file is uploaded.</param>
        /// <param name="agentName">The agent name.</param>
        /// <param name="oneDriveWorkSchool">The OneDrive work or school item.</param>
        /// <returns></returns>
        [HttpPost("download")]
        public async Task<IActionResult> Download(string instanceId, string sessionId, string agentName, [FromBody] OneDriveWorkSchoolItem oneDriveWorkSchool)
        {
            var result = await _oneDriveWorkSchoolService.Download(instanceId, sessionId, agentName, oneDriveWorkSchool, _callContext.CurrentUserIdentity!);

            return Ok(result);
        }
    }
}
