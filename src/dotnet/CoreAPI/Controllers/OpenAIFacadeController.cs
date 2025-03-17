using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides facade methods for OpenAI-compatible API endpoints.
    /// </summary>
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = AuthorizationPolicyNames.MicrosoftEntraIDStandard)]
    [ApiController]
    [Route("instances/{instanceId}")]
    public class OpenAIFacadeController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly ILogger<OpenAIFacadeController> _logger;
        private readonly ICallContext _callContext;

        /// <summary>
        /// Constructor for the OpenAI Facade Controller.
        /// </summary>
        /// <param name="coreService">The Core service provides methods for getting completions from the orchestrator.</param>
        /// <param name="callContext">The call context for the request.</param>
        /// <param name="logger">The logging interface used to log under the OpenAIFacadeController type name.</param>
        public OpenAIFacadeController(
            ICoreService coreService,
            ICallContext callContext,
            ILogger<OpenAIFacadeController> logger)
        {
            _coreService = coreService;
            _logger = logger;
            _callContext = callContext;
        }

        /// <summary>
        /// Requests a completion using OpenAI-compatible format.
        /// </summary>
        /// <param name="instanceId">The instance ID of the current request.</param>
        /// <param name="agentName">The name of the agent to use for completion.</param>
        /// <param name="completionRequest">The completion request in OpenAI-compatible format.</param>
        [HttpPost("openai/{agentName}/completions")]
        public async Task<ActionResult<OpenAIFacadeCompletionResponse>> GetCompletion(
            string instanceId,
            string agentName,
            [FromBody] OpenAIFacadeCompletionRequest facadeRequest)
        {
            facadeRequest.Model = agentName;
            var completionRequest = facadeRequest.ToCompletionRequest();
            completionRequest.OperationId = Guid.NewGuid().ToString().ToLower();
            
            var result = await _coreService.GetRawCompletionAsync(instanceId, completionRequest);
            var response = OpenAIFacadeCompletionResponse.FromCompletionResponse(result);
            return Ok(response);
        }

        /// <summary>
        /// Requests a chat completion using OpenAI-compatible format.
        /// </summary>
        /// <param name="instanceId">The instance ID of the current request.</param>
        /// <param name="agentName">The name of the agent to use for chat completion.</param>
        /// <param name="chatRequest">The chat completion request in OpenAI-compatible format.</param>
        [HttpPost("openai/{agentName}/chat/completions")]
        public async Task<ActionResult<OpenAIFacadeChatCompletionResponse>> GetChatCompletion(
            string instanceId,
            string agentName,
            [FromBody] OpenAIFacadeChatCompletionRequest chatRequest)
        {
            chatRequest.Model = agentName;
            var completionRequest = chatRequest.ToCompletionRequest();
            completionRequest.OperationId = Guid.NewGuid().ToString().ToLower();
            
            var result = await _coreService.GetRawChatCompletionAsync(instanceId, completionRequest);
            var response = OpenAIFacadeChatCompletionResponse.FromCompletionResponse(result);
            return Ok(response);
        }
    }
} 
