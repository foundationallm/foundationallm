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
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

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

        protected override async Task ExpandAndValidateAgent()
        {
            var agent = _request.Agent as KnowledgeManagementAgent;

            if (agent!.OrchestrationSettings!.AgentParameters == null)
                throw new SemanticKernelException("The agent parameters are missing in the orchestration settings.", StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(agent.Vectorization.TextEmbeddingProfileObjectId))
                throw new SemanticKernelException("Invalid text embedding profile object id.", StatusCodes.Status400BadRequest);

            if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                    agent.Vectorization.TextEmbeddingProfileObjectId, out var textEmbeddingProfileObject))
                throw new SemanticKernelException("The text embedding profile object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

            if (textEmbeddingProfileObject is not TextEmbeddingProfile textEmbeddingProfile
                || textEmbeddingProfile.ConfigurationReferences == null
                || !textEmbeddingProfile.ConfigurationReferences.TryGetValue("DeploymentName", out var deploymentNameConfigurationItem)
                || string.IsNullOrWhiteSpace(deploymentNameConfigurationItem)
                || !textEmbeddingProfile.ConfigurationReferences.TryGetValue("Endpoint", out var textEmbeddingEndpointConfigurationItem)
                || string.IsNullOrWhiteSpace(textEmbeddingEndpointConfigurationItem))
                throw new SemanticKernelException("The text embedding profile object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

            _textEmbeddingDeploymentName = await GetConfigurationValue(deploymentNameConfigurationItem);
            _textEmbeddingEndpoint = await GetConfigurationValue(textEmbeddingEndpointConfigurationItem);

            if (string.IsNullOrWhiteSpace(agent.Vectorization.IndexingProfileObjectId))
                throw new SemanticKernelException("Invalid indexing profile object id.", StatusCodes.Status400BadRequest);

            if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                    agent.Vectorization.IndexingProfileObjectId, out var indexingProfileObject))
                throw new SemanticKernelException("The indexing profile object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

            if (indexingProfileObject is not IndexingProfile indexingProfile
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

            if (string.IsNullOrWhiteSpace(agent.PromptObjectId))
                throw new SemanticKernelException("Invalid prompt object id.", StatusCodes.Status400BadRequest);

            if (!agent.OrchestrationSettings.AgentParameters.TryGetValue(
                    agent.PromptObjectId, out var promptObject))
                throw new SemanticKernelException("The prompt object is missing from the agent parameters.", StatusCodes.Status400BadRequest);

            if (promptObject is not MultipartPrompt prompt
                || string.IsNullOrWhiteSpace(prompt.Prefix))
                throw new SemanticKernelException("The prompt object provided in the agent parameters is invalid.", StatusCodes.Status400BadRequest);

            _prompt = prompt.Prefix;
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
            var kernel = builder.Build();

            var memory = new MemoryBuilder()
                .WithMemoryStore(new AzureAISearchMemoryStore(_indexingEndpoint, credential))
                .WithAzureOpenAITextEmbeddingGeneration(_textEmbeddingDeploymentName, _textEmbeddingEndpoint, credential)
                .Build();

            kernel.ImportPluginFromObject(new KnowledgeManagementContextPlugin(memory, _indexName));

            return kernel;
        }
    }
}
