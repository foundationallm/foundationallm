using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.SemanticKernel.Core.Exceptions;
using FoundationaLLM.SemanticKernel.Core.Filters;
using FoundationaLLM.SemanticKernel.Core.Plugins;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using System.Net;
using System.Text.Json;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0020, SKEXP0050, SKEXP0060

namespace FoundationaLLM.SemanticKernel.Core.Agents
{
    /// <summary>
    /// The Knowledge Management agent.
    /// </summary>
    /// <param name="request">The <see cref="KnowledgeManagementCompletionRequest"/> to be processed by the agent.</param>
    /// <param name="resourceProviderServices">A collection of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    public class SemanticKernelKnowledgeManagementAgent(
        LLMCompletionRequest request,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILoggerFactory loggerFactory) : SemanticKernelAgentBase(request, resourceProviderServices, loggerFactory.CreateLogger<SemanticKernelKnowledgeManagementAgent>())
    {
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        private string _textEmbeddingDeploymentName = string.Empty;
        private string _textEmbeddingEndpoint = string.Empty;
        private string _indexerName = string.Empty;
        private string _indexingEndpoint = string.Empty;
        private string _indexName = string.Empty;
        private string _prompt = string.Empty;
        private Dictionary<string, string>? _agentDescriptions = [];

        protected override async Task ExpandAndValidateAgent()
        {
            var agent = _request.Agent as KnowledgeManagementAgent;

            if (agent!.OrchestrationSettings!.AgentParameters == null)
                throw new SemanticKernelException("The agent parameters are missing in the orchestration settings.", StatusCodes.Status400BadRequest);

            #region Other agent descriptions

            if (agent.OrchestrationSettings.AgentParameters.TryGetValue(
                        "AllAgents", out var allAgentDescriptions))
            {
                _agentDescriptions = allAgentDescriptions is JsonElement allAgentDescriptionsJsonElement
                    ? allAgentDescriptionsJsonElement.Deserialize<Dictionary<string, string>>()
                    : allAgentDescriptions as Dictionary<string, string>;
            }

            #endregion

            #region Prompt

            if (string.IsNullOrWhiteSpace(agent.PromptObjectId))
                throw new SemanticKernelException("Invalid prompt object id.", StatusCodes.Status400BadRequest);

            if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                    agent.PromptObjectId, out var promptObject))
                throw new SemanticKernelException("The prompt object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

            var prompt = promptObject is JsonElement promptJsonElement
                ? promptJsonElement.Deserialize<MultipartPrompt>()
                : promptObject as MultipartPrompt;

            if (prompt == null
                || string.IsNullOrWhiteSpace(prompt.Prefix))
                throw new SemanticKernelException("The prompt object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

            _prompt = prompt.Prefix;

            #endregion

            #region Vectorization (text embedding and indexing) - optional

            if (!string.IsNullOrWhiteSpace(agent.Vectorization.TextEmbeddingProfileObjectId))
            {
                if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                        agent.Vectorization.TextEmbeddingProfileObjectId, out var textEmbeddingProfileObject))
                    throw new SemanticKernelException("The text embedding profile object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

                var textEmbeddingProfile = textEmbeddingProfileObject is JsonElement textEmbeddingProfileJsonElement
                    ? textEmbeddingProfileJsonElement.Deserialize<TextEmbeddingProfile>()
                    : textEmbeddingProfileObject as TextEmbeddingProfile;

                if (textEmbeddingProfile == null
                    || textEmbeddingProfile.ConfigurationReferences == null
                    || !textEmbeddingProfile.ConfigurationReferences.TryGetValue("DeploymentName", out var deploymentNameConfigurationItem)
                    || string.IsNullOrWhiteSpace(deploymentNameConfigurationItem)
                    || !textEmbeddingProfile.ConfigurationReferences.TryGetValue("Endpoint", out var textEmbeddingEndpointConfigurationItem)
                    || string.IsNullOrWhiteSpace(textEmbeddingEndpointConfigurationItem))
                    throw new SemanticKernelException("The text embedding profile object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

                _textEmbeddingDeploymentName = textEmbeddingProfile.Settings != null
                    && textEmbeddingProfile.Settings.TryGetValue("deployment_name", out string? deploymentNameOverride)
                    && !string.IsNullOrWhiteSpace(deploymentNameOverride)
                    ? deploymentNameOverride
                    : await GetConfigurationValue(deploymentNameConfigurationItem);
                _textEmbeddingEndpoint = await GetConfigurationValue(textEmbeddingEndpointConfigurationItem);
            }

            if (!string.IsNullOrWhiteSpace(agent.Vectorization.IndexingProfileObjectId))
            {

                if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                        agent.Vectorization.IndexingProfileObjectId, out var indexingProfileObject))
                    throw new SemanticKernelException("The indexing profile object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

                var indexingProfile = indexingProfileObject is JsonElement indexingProfileJsonElement
                    ? indexingProfileJsonElement.Deserialize<IndexingProfile>()
                    : indexingProfileObject as IndexingProfile;

                if (indexingProfile == null
                    || indexingProfile.ConfigurationReferences == null
                    || !indexingProfile.ConfigurationReferences.TryGetValue("Endpoint", out var indexingEndpointConfigurationItem)
                    || string.IsNullOrWhiteSpace(indexingEndpointConfigurationItem)
                    || indexingProfile.Settings == null
                    || !indexingProfile.Settings.TryGetValue("IndexName", out var indexName)
                    || string.IsNullOrWhiteSpace(indexName))
                    throw new SemanticKernelException("The indexing profile object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

                _indexerName = indexingProfile.Indexer.ToString();
                _indexName = indexName;
                _indexingEndpoint = await GetConfigurationValue(indexingEndpointConfigurationItem);
            }

            #endregion
        }

        protected override async Task<LLMCompletionResponse> BuildResponseWithAzureOpenAI()
        {
            try
            {
                var kernel = BuildKernel();

                // Use observability features to capture the fully rendered prompt.
                var promptFilter = new DefaultPromptFilter();
                kernel.PromptFilters.Add(promptFilter);

                var arguments = new KernelArguments()
                {
                    ["userPrompt"] = _request.UserPrompt!,
                    ["messageHistory"] = _request.MessageHistory
                };

                var result = await kernel.InvokePromptAsync(_prompt, arguments);

                var completion = result.GetValue<string>()!;
                var completionUsage = (result.Metadata!["Usage"] as CompletionsUsage)!;

                return new LLMCompletionResponse
                {
                    Completion = completion,
                    UserPrompt = _request.UserPrompt!,
                    FullPrompt = promptFilter.RenderedPrompt,
                    AgentName = _request.Agent.Name,
                    PromptTokens = completionUsage!.PromptTokens,
                    CompletionTokens = completionUsage.CompletionTokens,
                    TotalTokens = completionUsage.TotalTokens
                };
            }
            catch (Exception ex)
            {
                var message = "The response building process encountered an error.";
                _logger.LogError(ex, message);
                throw new SemanticKernelException(message, StatusCodes.Status500InternalServerError);
            }
        }

        private Kernel BuildKernel()
        {
            var credential = DefaultAuthentication.GetAzureCredential();

            var builder = Kernel.CreateBuilder();
            builder.Services.AddSingleton<ILoggerFactory>(_loggerFactory);
            builder.AddAzureOpenAIChatCompletion(
                _deploymentName,
                _endpoint,
                credential);
            builder.Services.ConfigureHttpClientDefaults(c =>
            {
                // Use a standard resiliency policy configured to retry on 429 (too many requests).
                c.AddStandardResilienceHandler().Configure(o =>
                {
                    o.Retry.ShouldHandle = args => ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
                });
            });
            var kernel = builder.Build();

            // If the vectorization properties are not set, we are not going to import the context building capabilities

            if (!string.IsNullOrWhiteSpace(_indexingEndpoint))
            {
                var memory = new MemoryBuilder()
                    .WithMemoryStore(new AzureAISearchMemoryStore(_indexingEndpoint, credential))
                    .WithAzureOpenAITextEmbeddingGeneration(_textEmbeddingDeploymentName, _textEmbeddingEndpoint, credential)
                    .Build();

                kernel.ImportPluginFromObject(new KnowledgeManagementContextPlugin(memory, _indexName));
            }

            if (_agentDescriptions != null && _agentDescriptions.Count > 0)
            {
                kernel.ImportPluginFromObject(new AgentConversationPlugin(_agentDescriptions));
            }

            return kernel;
        }
    }
}
