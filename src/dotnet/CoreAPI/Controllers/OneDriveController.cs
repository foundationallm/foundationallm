using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for OneDrive integration.
    /// </summary>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("instances/{instanceId}/[controller]")]
    public class OneDriveController : ControllerBase
    {
        private readonly ICallContext _callContext;
        private readonly IOneDriveService _oneDriveService;

        /// <summary>
        /// The controller for OneDrive integration.
        /// </summary>
        /// <param name="callContext">The <see cref="ICallContext"/> call context of the request being handled.</param>
        /// <param name="oneDriveService">The <see cref="IOneDriveService"/> OneDrive service.</param>
        /// <exception cref="ResourceProviderException"></exception>
        public OneDriveController(
            ICallContext callContext,
            IOneDriveService oneDriveService)
        {
            _callContext = callContext;
            _oneDriveService = oneDriveService;
        }

        /// <summary>
        /// Connects to user's OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <returns></returns>
        [HttpPost("connect")]
        public async Task<IActionResult> Connect(string instanceId)
        {
            await _oneDriveService.Connect(instanceId);

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
            await _oneDriveService.Disconnect(instanceId);

            return Ok();
        }

        /// <summary>
        /// Downloads a file from the user's connected OneDrive work or school account.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="itemId">The OneDrive work or school item identifier.</param>
        /// <returns>The file contents.</returns>
        [HttpPost("download")]
        public async Task<IActionResult> Download(string instanceId, [FromQuery] string itemId)
        {
            var content = await _oneDriveService.Download(instanceId, itemId, _callContext.CurrentUserIdentity!);

            return Ok(content);
        }
    }
}
