using Azure.AI.OpenAI;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FoundationaLLM.SemanticKernel.Core.Plugins
{
    public class KnowledgeManagementAgentPlugin
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public KnowledgeManagementAgentPlugin(
            ILogger<KnowledgeManagementAgentPlugin> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(LLMCompletionRequest request)
        {
            var kernel = CreateKernel(request.Agent.OrchestrationSettings!, request.Settings!);

            ChatHistory history = [];

            var internalContext = true;
            var messageHistoryEnabled = false;

            var kmAgent = request.Agent as KnowledgeManagementAgent;

            if (kmAgent?.Vectorization.IndexingProfileObjectId != null && kmAgent.Vectorization.TextEmbeddingProfileObjectId != null)
            {
                internalContext = false;
            }

            if (request.Agent.ConversationHistory != null)
            {
                if (request.Agent.ConversationHistory.Enabled)
                {
                    messageHistoryEnabled = true;
                }
            }

            //var agentPrompt = ResourceProviderService.GetAgentPrompt(request.Agent.Prompt);
            var agentPrompt = string.Empty;
            var context = string.Empty;
            var promptBuilder = $"{context}";

            if (!internalContext)
            {
                promptBuilder = agentPrompt;
                if (messageHistoryEnabled)
                {
                    promptBuilder = $"\n\nQuestion: {request.UserPrompt}\n\nContext: {context}\n\nAnswer:";
                }
            }

            if (messageHistoryEnabled)
                history.AddUserMessage(request.UserPrompt!);

            var modelVersion = request.Agent.OrchestrationSettings!.ModelParameters?.GetValueOrDefault(ModelParameterKeys.Version)?.ToString();

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(promptBuilder, new PromptExecutionSettings() { ModelId = modelVersion });
            var usage = result.Metadata!["Usage"] as CompletionsUsage;

            if (messageHistoryEnabled)
                history.AddAssistantMessage(result.Content!);

            return new LLMCompletionResponse()
            {
                Completion = result.Content,
                UserPrompt = request.UserPrompt,
                FullPrompt = promptBuilder,
                PromptTemplate = "\n\nQuestion: {question}\n\nContext: {context}\n\nAnswer:",
                AgentName = request.Agent.Name,
                PromptTokens = usage!.PromptTokens,
                CompletionTokens = usage.CompletionTokens,
                TotalTokens = usage.TotalTokens,
                TotalCost = 0
            };
        }

        private Kernel CreateKernel(OrchestrationSettings agentSettings, OrchestrationSettings overrideSettings)
        {
            var deploymentName = agentSettings.ModelParameters!.GetValueOrDefault(ModelParameterKeys.DeploymentName)?.ToString();
            if (deploymentName.IsNullOrEmpty())
                throw new ArgumentException("A valid deployment name is required to be set in the agent's OrchestrationSettings.ModelParameters.");

            if (!agentSettings.EndpointConfiguration!.TryGetValue(EndpointConfigurationKeys.Endpoint, out var endpointKeyName))
                throw new Exception("An endpoint value must be passed in via an Azure App Config key name.");

            var endpoint = _configuration.GetValue<string>(endpointKeyName?.ToString()!);

            if (!agentSettings.EndpointConfiguration.TryGetValue(EndpointConfigurationKeys.APIKey, out var apiKeyKeyName))
                throw new Exception("An API key value must be passed in via an Azure App Config key name.");

            var apiKey = _configuration.GetValue<string>(apiKeyKeyName?.ToString()!);

            agentSettings.EndpointConfiguration.TryGetValue(EndpointConfigurationKeys.Provider, out var providerKeyName);
            var provider = _configuration.GetValue<string>(providerKeyName?.ToString() ?? LanguageModelProviders.MICROSOFT);

            agentSettings.EndpointConfiguration.TryGetValue(EndpointConfigurationKeys.OperationType, out var operationTypeKeyName);
            var operationType = _configuration.GetValue<string>(operationTypeKeyName?.ToString() ?? OperationTypes.Chat);

            var builder = Kernel.CreateBuilder();

            if (provider == LanguageModelProviders.MICROSOFT)
            {
                if (operationType == OperationTypes.Chat)
                    builder.AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);
                else
                    builder.AddAzureOpenAITextGeneration(deploymentName!, endpoint!, apiKey!);
            }
            else
            {
                if (operationType == OperationTypes.Chat)
                    builder.AddOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);
                else
                    builder.AddOpenAITextGeneration(deploymentName!, endpoint!, apiKey!);
            }

            return builder.Build();
        }
    }
}
