using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Knowledge Management orchestration.
    /// </summary>
    /// <remarks>
    /// Constructor for default agent.
    /// </remarks>
    /// <param name="agent">The <see cref="KnowledgeManagementAgent"/> agent.</param>
    /// <param name="callContext">The call context of the request being handled.</param>
    /// <param name="orchestrationService"></param>
    /// <param name="logger">The logger used for logging.</param>
    public class KnowledgeManagementOrchestration(
        KnowledgeManagementAgent agent,
        ICallContext callContext,
        ILLMOrchestrationService orchestrationService,
        ILogger<OrchestrationBase> logger,
        Dictionary<string, IResourceProviderService> resourceProviderServices) : OrchestrationBase(orchestrationService)
    {
        private readonly ICallContext _callContext = callContext;
        private readonly ILogger<OrchestrationBase> _logger = logger;
        private readonly KnowledgeManagementAgent _agent = agent;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices = resourceProviderServices;

        /// <inheritdoc/>
        public override async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            var result = await _orchestrationService.GetCompletion(
                new LLMCompletionRequest
                {
                    UserPrompt = completionRequest.UserPrompt!,
                    Agent = _agent,
                    MessageHistory = completionRequest.MessageHistory,
                    Settings = completionRequest.Settings,
                    AttachmentDataLakeUrls = await GetUrlFromObjectId(completionRequest.AttachmentObjectIds)
                });

            if (result.Citations != null)
            {
                result.Citations = result.Citations
                    .GroupBy(c => c.Filepath)
                    .Select(g => g.First())
                    .ToArray();
            }

            return new CompletionResponse
            {
                Completion = result.Completion!,
                UserPrompt = completionRequest.UserPrompt!,
                Citations = result.Citations,
                FullPrompt = result.FullPrompt,
                PromptTemplate = result.PromptTemplate,
                AgentName = result.AgentName,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
            };
        }

        private async Task<List<string>> GetUrlFromObjectId(List<string>? attachmentObjectIds)
        {
            //if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Attachment, out var attachmentResourceProvider))
            //    throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Attachment} was not loaded.");

            List<string> result = new List<string>();
            //foreach(var attachmentObjectId in attachmentObjectIds)
            //{
            //    var attachment = await attachmentResourceProvider.GetResource<Attachment>(
            //       attachmentObjectId,
            //       _callContext.CurrentUserIdentity);

            //    result.Add(attachmentURL);
            //}
            return result;
        }
    }
}
