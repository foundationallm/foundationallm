using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides WebSocket endpoints for realtime speech-to-speech communication.
    /// </summary>
    /// <remarks>
    /// This controller supports Microsoft Entra ID authentication for interactive users.
    /// WebSocket connections require token to be passed via query string (access_token parameter)
    /// since WebSockets cannot use standard Authorization headers.
    /// </remarks>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Route("instances/{instanceId}")]
    public class RealtimeSpeechController : ControllerBase
    {
        private readonly IRealtimeSpeechService _realtimeSpeechService;
        private readonly ILogger<RealtimeSpeechController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealtimeSpeechController"/> class.
        /// </summary>
        /// <param name="realtimeSpeechService">Service for realtime speech operations.</param>
        /// <param name="logger">Logger for controller diagnostics.</param>
        public RealtimeSpeechController(
            IRealtimeSpeechService realtimeSpeechService,
            ILogger<RealtimeSpeechController> logger)
        {
            _realtimeSpeechService = realtimeSpeechService;
            _logger = logger;
        }

        /// <summary>
        /// Establishes a WebSocket connection for realtime speech communication.
        /// </summary>
        [HttpGet("sessions/{sessionId}/realtime-speech")]
        public async Task<IActionResult> ConnectRealtimeSpeech(
            string instanceId,
            string sessionId,
            [FromQuery] string agentName)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                return BadRequest("WebSocket connection required");
            }

            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            
            try
            {
                await _realtimeSpeechService.HandleWebSocketConnection(
                    instanceId,
                    sessionId,
                    agentName,
                    webSocket,
                    HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in realtime speech WebSocket connection");
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.InternalServerError,
                        "Internal error",
                        CancellationToken.None);
                }
            }

            return new EmptyResult();
        }

        /// <summary>
        /// Gets the realtime speech configuration for an agent.
        /// </summary>
        [HttpGet("agents/{agentName}/realtime-speech/config")]
        public async Task<IActionResult> GetRealtimeSpeechConfig(
            string instanceId,
            string agentName)
        {
            var config = await _realtimeSpeechService.GetConfigurationAsync(
                instanceId,
                agentName);
            
            return Ok(config);
        }
    }
}
