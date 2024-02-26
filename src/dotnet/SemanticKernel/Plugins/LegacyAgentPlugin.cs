using Azure.AI.OpenAI;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FoundationaLLM.SemanticKernel.Core.Plugins
{
    public class LegacyAgentPlugin : ILegacyAgentPlugin
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public LegacyAgentPlugin(
            ILogger<LegacyAgentPlugin> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(LegacyCompletionRequest request)
        {
            var kernel = CreateKernel(request.LanguageModel!);

            var modelVersion = _configuration.GetValue<string>(request.LanguageModel!.Version!);

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(request.UserPrompt!, new PromptExecutionSettings() { ModelId = modelVersion });
            var usage = result.Metadata!["Usage"] as CompletionsUsage;

            return new LLMCompletionResponse()
            {
                Completion = result.Content,
                UserPrompt = request.UserPrompt,
                FullPrompt = request.UserPrompt,
                PromptTemplate = request.UserPrompt,
                AgentName = request.Agent!.Name,
                PromptTokens = usage!.PromptTokens,
                CompletionTokens = usage.CompletionTokens,
                TotalTokens = usage.TotalTokens,
                TotalCost = 0
            };
        }

        private Kernel CreateKernel(LanguageModel llm)
        {
            var deploymentName = _configuration.GetValue<string>(llm.Deployment!);
            var endpoint = _configuration.GetValue<string>(llm.ApiEndpoint!);
            var apiKey = _configuration.GetValue<string>(llm.ApiKey!);

            var builder = Kernel.CreateBuilder();

            if (llm.Provider == LanguageModelProviders.MICROSOFT)
            {
                if (llm.UseChat)
                    builder.AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);
                else
                    builder.AddAzureOpenAITextGeneration(deploymentName!, endpoint!, apiKey!);
            }
            else
            {
                if (llm.UseChat)
                    builder.AddOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);
                else
                    builder.AddOpenAITextGeneration(deploymentName!, endpoint!, apiKey!);
            }

            return builder.Build();
        }
    }
}
