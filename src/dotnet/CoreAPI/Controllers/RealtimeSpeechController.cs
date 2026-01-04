using FoundationaLLM.Common.Authentication;
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
    /// This controller supports both Microsoft Entra ID and Agent Access Token authentication,
    /// enabling use by both interactive users and API clients with agent access tokens.
    /// </remarks>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [Authorize(
        AuthenticationSchemes = AgentAccessTokenDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.FoundationaLLMAgentAccessToken)]
    [ApiController]
    [Route("instances/{instanceId}")]
    public class RealtimeSpeechController : ControllerBase
    {
        private readonly IRealtimeSpeechService _realtimeSpeechService;
        private readonly ILogger<RealtimeSpeechController> _logger;

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
