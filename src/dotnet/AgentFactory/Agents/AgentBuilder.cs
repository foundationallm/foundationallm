﻿using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Agents
{
    /// <summary>
    /// Agent Builder class
    /// </summary>
    public class AgentBuilder
    {
        /// <summary>
        /// Used to build an agenet given the inbound parameters.
        /// </summary>
        /// <param name="userPrompt"></param>
        /// <param name="sessionId"></param>
        /// <param name="agentHubAPIService"></param>
        /// <param name="orchestrationServices"></param>
        /// <param name="promptHubAPIService"></param>
        /// <param name="dataSourceHubAPIService"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<AgentBase> Build(
            string userPrompt,
            string sessionId,
            IAgentHubAPIService agentHubAPIService,
            IEnumerable<ILLMOrchestrationService> orchestrationServices,
            IPromptHubAPIService promptHubAPIService,
            IDataSourceHubAPIService dataSourceHubAPIService)
        {
            var agentResponse = await agentHubAPIService.ResolveRequest(userPrompt, sessionId);
            var agentInfo = agentResponse.Agent;

            // TODO: Extend the Agent Hub API service response to include the orchestrator
            var orchestrationType = string.IsNullOrWhiteSpace(agentResponse.Agent!.Orchestrator) 
                ? "LangChain"
                : agentInfo!.Orchestrator;

            var validType = Enum.TryParse<LLMOrchestrationService>(orchestrationType, out LLMOrchestrationService llmOrchestrationType);
            if (!validType)
                throw new ArgumentException($"The agent factory does not support the {orchestrationType} orchestration type.");
            var orchestrationService = SelectOrchestrationService(llmOrchestrationType, orchestrationServices);

            
            AgentBase? agent = null;
            agent = new DefaultAgent(agentInfo!, orchestrationService, promptHubAPIService, dataSourceHubAPIService);           

            await agent.Configure(userPrompt, sessionId);

            return agent;
        }

        /// <summary>
        /// Used to select the orchestration service for the agent.
        /// </summary>
        /// <param name="orchestrationType"></param>
        /// <param name="orchestrationServices"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        static ILLMOrchestrationService SelectOrchestrationService(
            LLMOrchestrationService orchestrationType,
            IEnumerable<ILLMOrchestrationService> orchestrationServices)
        {
            Type? orchestrationServiceType = null;

            switch (orchestrationType)
            {
                case LLMOrchestrationService.LangChain:
                    orchestrationServiceType = typeof(ILangChainService);
                    break;
                case LLMOrchestrationService.SemanticKernel:
                    orchestrationServiceType = typeof(ISemanticKernelService);
                    break;
                default:
                    throw new ArgumentException($"The orchestration type {orchestrationType} is not supported.");
            }

            var orchestrationService = orchestrationServices.FirstOrDefault(x => orchestrationServiceType.IsAssignableFrom(x.GetType()));
            if (orchestrationService == null)
                throw new ArgumentException($"There is no orchestration service available for orchestration type {orchestrationType}.");

            return orchestrationService;
        }
    }
}
