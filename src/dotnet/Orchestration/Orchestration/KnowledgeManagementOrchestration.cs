using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
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
    /// <param name="resourceProviderServices">The dictionary of <see cref="IResourceProviderService"/></param>
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
                    Attachments = completionRequest.Attachments == null ? [] : await GetAttachmentPaths(completionRequest.Attachments),
                    Settings = completionRequest.Settings
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

        private async Task<List<string>> GetAttachmentPaths(List<string> attachmentObjectIds)
        {
            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Attachment, out var attachmentResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Attachment} was not loaded.");

            var attachments = attachmentObjectIds
                .ToAsyncEnumerable()
                .SelectAwait(async x => await attachmentResourceProvider.GetResource<AttachmentFile>(x, _callContext.CurrentUserIdentity!));

            List<string> result = [];
            await foreach (var attachment in attachments)
                result.Add(attachment.Path);

            return result;
        }
    }
}
