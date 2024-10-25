using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    /// <summary>
    /// Wrapper for the Semantic Kernel service.
    /// </summary>
    /// <param name="semanticKernelService">The Semantic Kernel service handling requests.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}")]
    public class CompletionsController(
        ISemanticKernelService semanticKernelService) : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService = semanticKernelService;

        /// <summary>
        /// Begins a completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>Returns an <see cref="LongRunningOperation"/> object containing the OperationId and Status.</returns>
        [HttpPost("async-completions")]
        public async Task<ActionResult<LongRunningOperation>> StartCompletionOperation(string instanceId, LLMCompletionRequest completionRequest)
        {
            var longRunningOperation = await _semanticKernelService.StartCompletionOperation(instanceId, completionRequest);
            return Accepted(longRunningOperation);
        }

        /// <summary>
        /// Gets the status of a completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="operationId">The OperationId for which to retrieve the status.</param>
        /// <returns>Returns an <see cref="LongRunningOperation"/> object containing the OperationId and Status.</returns>
        [HttpGet("async-completions/{operationId}/status")]
        public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId) =>
            await _semanticKernelService.GetCompletionOperationStatus(instanceId, operationId);

        /// <summary>
        /// Gets a completion operation from the Orchestration service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="operationId">The ID of the operation to retrieve.</param>
        /// <returns>A <see cref="LLMCompletionResponse"/> containing the response to the completion request.</returns>
        [HttpGet("async-completions/{operationId}/result")]
        public async Task<LLMCompletionResponse> GetCompletionOperationResult(string instanceId, string operationId) =>
            await _semanticKernelService.GetCompletionOperationResult(instanceId, operationId);
    }
}
