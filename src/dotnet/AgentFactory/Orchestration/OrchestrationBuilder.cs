using FoundationaLLM.Agent.Constants;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Cache;
using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.AgentFactory.Core.Orchestration
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
        /// <param name="cacheService">The <see cref="ICacheService"/> used to cache agent-related artifacts.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="agentHubAPIService"></param>
        /// <param name="orchestrationServices"></param>
        /// <param name="promptHubAPIService"></param>
        /// <param name="dataSourceHubAPIService"></param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> Build(
            CompletionRequest completionRequest,
            ICacheService cacheService,
            ICallContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            IAgentHubAPIService agentHubAPIService,
            IEnumerable<ILLMOrchestrationService> orchestrationServices,
            IPromptHubAPIService promptHubAPIService,
            IDataSourceHubAPIService dataSourceHubAPIService,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();
            if (completionRequest.Settings?.AgentName == null)
                logger.LogInformation("The AgentBuilder is starting to build an agent without an agent name.");
            else
                logger.LogInformation("The AgentBuilder is starting to build an agent with the following agent name: {AgentName}.",
                    completionRequest.Settings?.AgentName);

            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");

            // TODO: Implement a cleaner pattern for handling missing resources
            AgentBase? agentBase = default;

            if (!string.IsNullOrWhiteSpace(completionRequest.Settings?.AgentName))
            {
                try
                {
                    var agents = await agentResourceProvider.HandleGetAsync($"/{AgentResourceTypeNames.Agents}/{completionRequest.Settings?.AgentName}");
                    agentBase = ((List<AgentBase>)agents)[0];
                }
                catch (ResourceProviderException)
                {
                    logger.LogInformation("AgentBuilder: The requested agent was not found in the resource provider, defaulting to legacy agent path.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "The AgentBuilder failed to properly retrieve the agent: /{Agents}/{AgentName}",
                        AgentResourceTypeNames.Agents, completionRequest.Settings?.AgentName);
                    throw;
                }
            }

            ILLMOrchestrationService? orchestrationService = null;

            if (agentBase == null)
            {
                // Using the old way to build agents

                //var agentResponse = callContext.AgentHint != null
                //    ? await cacheService.Get<AgentHubResponse>(
                //        new CacheKey(callContext.AgentHint.Name!, CacheCategories.Agent),
                //        async () => { return await agentHubAPIService.ResolveRequest(
                //            completionRequest.UserPrompt,
                //            completionRequest.SessionId ?? string.Empty); },
                //        false,
                //        TimeSpan.FromHours(1))
                //    : await agentHubAPIService.ResolveRequest(
                //        completionRequest.UserPrompt,
                //        completionRequest.SessionId ?? string.Empty);

                //var agentInfo = agentResponse!.Agent;

                //if (agentResponse is { Agent: not null })
                //{
                //    logger.LogInformation("The AgentBuilder received the following agent from the AgentHub: {AgentName}.",
                //        agentResponse.Agent!.Name);
                //}

                //// TODO: Extend the Agent Hub API service response to include the orchestrator
                //var orchestrationType = string.IsNullOrWhiteSpace(agentResponse.Agent!.Orchestrator)
                //    ? "LangChain"
                //    : agentInfo!.Orchestrator;

                //var validType = Enum.TryParse<LLMOrchestrationService>(orchestrationType, out LLMOrchestrationService llmOrchestrationType);
                //if (!validType)
                //    throw new ArgumentException($"The agent factory does not support the {orchestrationType} orchestration type.");
                //orchestrationService = SelectOrchestrationService(llmOrchestrationType, orchestrationServices);

                //OrchestrationBase? orchestration = null;
                //orchestration = new LegacyOrchestration(
                //    agentInfo!,
                //    cacheService, callContext,
                //    orchestrationService, promptHubAPIService, dataSourceHubAPIService,
                //    loggerFactory.CreateLogger<LegacyOrchestration>());

                //await orchestration.Configure(completionRequest);

                //return orchestration;

                return null;
            }

            if (agentBase.AgentType == typeof(KnowledgeManagementAgent) || agentBase.AgentType == typeof(InternalContextAgent))
            {
                var orchestrationType = string.IsNullOrWhiteSpace(agentBase.Orchestrator)
                    ? "LangChain"
                    : agentBase.Orchestrator;

                var validType = Enum.TryParse<LLMOrchestrationService>(orchestrationType, out LLMOrchestrationService llmOrchestrationType);
                if (!validType)
                    throw new ArgumentException($"The agent factory does not support the {orchestrationType} orchestration type.");
                orchestrationService = SelectOrchestrationService(llmOrchestrationType, orchestrationServices);

                // Hydrate overridable values from config and assign them back to the agent's LanguageModel.
                var deploymentName = configuration.GetValue<string>(agentBase.LanguageModel?.Deployment!);
                agentBase.LanguageModel!.Deployment = deploymentName;

                // Extract any override settings and apply them to the agent's LanguageModel.
                if (completionRequest.Settings?.ModelSettings != null)
                {
                    foreach (var key in completionRequest.Settings?.ModelSettings?.Keys!)
                    {
                        switch (key)
                        {
                            case ModelSettingsKeys.Deployment_Name:
                                agentBase.LanguageModel!.Deployment = completionRequest.Settings?.ModelSettings?.GetValueOrDefault(key)!.ToString();
                                break;
                            case ModelSettingsKeys.Temperature:
                                agentBase.LanguageModel!.Temperature = Convert.ToSingle(completionRequest.Settings?.ModelSettings?.GetValueOrDefault(key, 0f)!.ToString());
                                break;
                        }
                    }
                }

                if(agentBase.AgentType == typeof(KnowledgeManagementAgent))
                {
                    var kmOrchestration = new KnowledgeManagementOrchestration(
                        (KnowledgeManagementAgent)agentBase!,
                        cacheService,
                        callContext,
                        orchestrationService,
                        promptHubAPIService,
                        dataSourceHubAPIService,
                        loggerFactory.CreateLogger<OrchestrationBase>());
                    await kmOrchestration.Configure(completionRequest);

                    return kmOrchestration;
                }
                else
                {
                    var icOrchestration = new InternalContextOrchestration(
                        (InternalContextAgent)agentBase!,
                        cacheService, callContext,
                        orchestrationService,
                        promptHubAPIService,
                        dataSourceHubAPIService,
                        loggerFactory.CreateLogger<OrchestrationBase>());
                    await icOrchestration.Configure(completionRequest);

                    return icOrchestration;
                }

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
