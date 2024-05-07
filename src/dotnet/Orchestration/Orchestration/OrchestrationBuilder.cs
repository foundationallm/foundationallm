using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
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
        /// <param name="completionRequest">The <see cref="CompletionRequest"/> containing details about the completion request.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="orchestrationServices"></param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> Build(
            CompletionRequest completionRequest,
            ICallContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            IEnumerable<ILLMOrchestrationService> orchestrationServices,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();
            if (completionRequest.AgentName == null)
                logger.LogInformation("The AgentBuilder is starting to build an agent without an agent name.");
            else
                logger.LogInformation("The AgentBuilder is starting to build an agent with the following agent name: {AgentName}.",
                    completionRequest.AgentName);

            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");

            // TODO: Implement a cleaner pattern for handling missing resources
            AgentBase? agentBase = default;

            if (!string.IsNullOrWhiteSpace(completionRequest.AgentName))
            {
                try
                {
                    var agents = await agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}/{completionRequest.AgentName}", callContext.CurrentUserIdentity);
                    agentBase = ((List<AgentResourceProviderGetResult>)agents)[0].Agent;
                }
                catch (ResourceProviderException)
                {
                    logger.LogInformation("AgentBuilder: The requested agent was not found in the resource provider, defaulting to legacy agent path.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "The AgentBuilder failed to properly retrieve the agent: /{Agents}/{AgentName}",
                        AgentResourceTypeNames.Agents, completionRequest.AgentName);
                    throw;
                }
            }

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
                await kmOrchestration.Configure(completionRequest);

                return kmOrchestration;
            }

            return null;
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
