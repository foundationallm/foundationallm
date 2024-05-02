using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Builds an orchestration for a FoundationaLLM agent.
    /// </summary>
    public class OrchestrationBuilder
    {
        /// <summary>
        /// Builds the orchestration based on the user prompt, the session id, and the call context.
        /// </summary>
        /// <param name="agentName">The unique name of the agent for which the orchestration is built.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="orchestrationServices"></param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> Build(
            string agentName,
            ICallContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            IEnumerable<ILLMOrchestrationService> orchestrationServices,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();

            var agentBase = await LoadAgent(agentName, resourceProviderServices, callContext.CurrentUserIdentity!, logger);
            if (agentBase == null) return null;
            
            if (agentBase.AgentType == typeof(KnowledgeManagementAgent))
            {
                var orchestrationType = string.IsNullOrWhiteSpace(agentBase.OrchestrationSettings?.Orchestrator)
                    ? LLMOrchestrationService.LangChain.ToString()
                    : agentBase.OrchestrationSettings?.Orchestrator;

                var validType = Enum.TryParse(orchestrationType, out LLMOrchestrationService llmOrchestrationType);
                if (!validType)
                    throw new ArgumentException($"The orchestration does not support the {orchestrationType} orchestration type.");

                var orchestrationService = SelectOrchestrationService(llmOrchestrationType, orchestrationServices);
                
                var kmOrchestration = new KnowledgeManagementOrchestration(
                    (KnowledgeManagementAgent)agentBase,
                    callContext,
                    orchestrationService,
                    loggerFactory.CreateLogger<OrchestrationBase>());

                return kmOrchestration;
            }

            return null;
        }

        private static async Task<AgentBase> LoadAgent(
            string? agentName,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            UnifiedUserIdentity currentUserIdentity,
            ILogger<OrchestrationBuilder> logger)
        {
            if (string.IsNullOrWhiteSpace(agentName))
                throw new OrchestrationException("The agent name provided in the completion request is invalid.");

            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Prompt, out var promptResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vectorization} was not loaded.");

            var agents = await agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}/{agentName}", currentUserIdentity);
            var agentBase = ((List<AgentBase>)agents)[0];

            if (agentBase.OrchestrationSettings!.AgentParameters == null)
                agentBase.OrchestrationSettings.AgentParameters = [];

            var prompt = await GetResource<PromptBase>(
                agentBase.PromptObjectId!,
                PromptResourceTypeNames.Prompts,
                promptResourceProvider,
                currentUserIdentity);

            agentBase.OrchestrationSettings.AgentParameters[agentBase.PromptObjectId!] = prompt;

            var allAgents = await agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}", currentUserIdentity);
            var allAgentsDescriptions = ((List<AgentBase>)allAgents)
                .Where(a => !string.IsNullOrWhiteSpace(a.Description) && a.Name != agentBase.Name)
                .Select(a => new
                {
                    a.Name,
                    a.Description
                })
                .ToDictionary(x => x.Name, x => x.Description);
            agentBase.OrchestrationSettings.AgentParameters["AllAgents"] = allAgentsDescriptions;

            if (agentBase is KnowledgeManagementAgent kmAgent)
            {
                // check for inline-context/internal-context agents, they are valid KM agents that do not have a vectorization section.
                if(kmAgent.Vectorization != null)
                {
                    if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.IndexingProfileObjectId))
                    {
                        var indexingProfile = await GetResource<VectorizationProfileBase>(
                            kmAgent.Vectorization.IndexingProfileObjectId,
                            VectorizationResourceTypeNames.IndexingProfiles,
                            vectorizationResourceProvider,
                            currentUserIdentity);

                        kmAgent.OrchestrationSettings!.AgentParameters![kmAgent.Vectorization.IndexingProfileObjectId!] = indexingProfile;
                    }

                    if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.TextEmbeddingProfileObjectId))
                    {
                        var textEmbeddingProfile = await GetResource<VectorizationProfileBase>(
                            kmAgent.Vectorization.TextEmbeddingProfileObjectId,
                            VectorizationResourceTypeNames.TextEmbeddingProfiles,
                            vectorizationResourceProvider,
                            currentUserIdentity);

                        kmAgent.OrchestrationSettings!.AgentParameters![kmAgent.Vectorization.TextEmbeddingProfileObjectId!] = textEmbeddingProfile;
                    }
                }
            }

            return agentBase;
        }

        private static async Task<T> GetResource<T>(string objectId, string resourceTypeName, IResourceProviderService resourceProviderService, UnifiedUserIdentity currentUserIdentity)
            where T : ResourceBase
        {
            var result = await resourceProviderService.HandleGetAsync(
                $"/{resourceTypeName}/{objectId.Split("/").Last()}",
                currentUserIdentity);
            return (result as List<T>)!.First();
        }

        /// <summary>
        /// Used to select the orchestration service for the agent.
        /// </summary>
        /// <param name="orchestrationType"></param>
        /// <param name="orchestrationServices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static ILLMOrchestrationService SelectOrchestrationService(
            LLMOrchestrationService orchestrationType,
            IEnumerable<ILLMOrchestrationService> orchestrationServices)
        {
            Type? orchestrationServiceType = null;

            orchestrationServiceType = orchestrationType switch
            {
                LLMOrchestrationService.AzureAIDirect => typeof(IAzureAIDirectService),
                LLMOrchestrationService.AzureOpenAIDirect => typeof(IAzureOpenAIDirectService),
                LLMOrchestrationService.LangChain => typeof(ILangChainService),
                LLMOrchestrationService.SemanticKernel => typeof(ISemanticKernelService),
                _ => throw new ArgumentException($"The orchestration type {orchestrationType} is not supported."),
            };

            var orchestrationService = orchestrationServices.FirstOrDefault(x => orchestrationServiceType.IsAssignableFrom(x.GetType()));
            return orchestrationService
                ?? throw new ArgumentException($"There is no orchestration service available for orchestration type {orchestrationType}.");
        }
    }
}
