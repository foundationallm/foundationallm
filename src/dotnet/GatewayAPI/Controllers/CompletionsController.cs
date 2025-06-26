using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gateway.API.Controllers
{
    /// <summary>
    /// Methods for managing embedding requests.
    /// </summary>
    /// <param name="gatewayCore">The <see cref="IGatewayCore"/> that provides LLM gateway services.</param>
    /// <param name="callContext">The <see cref="IOrchestrationContext"/> call context of the request being handled.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/[controller]")]
    public class CompletionsController(
        IGatewayCore gatewayCore,
        IOrchestrationContext callContext)
    {
        private readonly IGatewayCore _gatewayCore = gatewayCore;
        private readonly IOrchestrationContext _callContext = callContext;

        /// <summary>
        /// Handles an incoming text completion request by starting a new completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="completionRequest">The <see cref="TextCompletionRequest"/> object with the details of the completion request.</param>
        /// <returns>A <see cref="TextOperationResult"/> object with the outcome of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> StartCompletionOperation(
            string instanceId,
            [FromBody] TextCompletionRequest completionRequest) =>
            new OkObjectResult(await _gatewayCore.StartCompletionOperation(instanceId, completionRequest, _callContext.CurrentUserIdentity!));

        /// <summary>
        /// Retrieves the outcome of a text embedding operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="operationId">The unique identifier of the text completion operation.</param>
        /// <returns>A <see cref="TextOperationResult"/> object with the outcome of the operation.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCompletionOperationResult(
            string instanceId,
            string operationId) =>
            new OkObjectResult(await _gatewayCore.GetCompletionOperationResult(instanceId, operationId, _callContext.CurrentUserIdentity!));
    }
}
