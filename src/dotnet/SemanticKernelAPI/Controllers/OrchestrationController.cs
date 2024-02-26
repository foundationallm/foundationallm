using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    /// <summary>
    /// Wrapper for the Semantic Kernel service.
    /// </summary>
    [ApiController]
    [APIKeyAuthentication]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IKnowledgeManagementAgentPlugin _knowledgeManagementAgentPlugin;
        private readonly ILegacyAgentPlugin _legacyAgentPlugin;

        /// <summary>
        /// Constructor for the Semantic Kernel API orchestration controller.
        /// </summary>
        /// <param name="knowledgeManagementAgentPlugin"></param>
        /// <param name="legacyAgentPlugin"></param>
        public OrchestrationController(
            IKnowledgeManagementAgentPlugin knowledgeManagementAgentPlugin,
            ILegacyAgentPlugin legacyAgentPlugin)
        {
            _knowledgeManagementAgentPlugin = knowledgeManagementAgentPlugin;
            _legacyAgentPlugin = legacyAgentPlugin;
        }

        /// <summary>
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        [HttpPost("completion")]
        public async Task<LLMCompletionResponse> GetCompletion([FromBody] LLMCompletionRequest request) => request switch
        {
            KnowledgeManagementCompletionRequest kmcr => await _knowledgeManagementAgentPlugin.GetCompletion(kmcr),
            LegacyCompletionRequest lcr => await _legacyAgentPlugin.GetCompletion(lcr),
            _ => throw new Exception($"LLM orchestration completion request of type {request.GetType()} is not supported."),
        };
    }
}
