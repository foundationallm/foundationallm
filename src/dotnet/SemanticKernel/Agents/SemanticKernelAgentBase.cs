using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.SemanticKernel.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.SemanticKernel.Core.Agents
{
    /// <summary>
    /// Implements the base functionality for a Semantic Kernel agent.
    /// </summary>
    /// <param name="request">The <see cref="LLMCompletionRequest"/> being processed by the agent.</param>
    /// <param name="resourceProviderServices">A collection of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class SemanticKernelAgentBase(
        LLMCompletionRequest request,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILogger logger)
    {
        protected readonly IResourceProviderService _configurationResourceProvider =
            resourceProviderServices.Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Configuration);

        protected readonly LLMCompletionRequest _request = request;
        protected readonly ILogger _logger = logger;

        protected string _llmProvider = string.Empty;
        protected string _endpointUrl = string.Empty;
        protected string _deploymentName = string.Empty;

        /// <summary>
        /// Gets the completion for the request.
        /// </summary>
        /// <returns>A <see cref="LLMCompletionResponse"/> object containing the completion response.</returns>
        public async Task<LLMCompletionResponse> GetCompletion()
        {
            try
            {
                _request.Validate();
                _deploymentName = _request.AIModel.DeploymentName!;
                _llmProvider = _request.AIModelEndpointConfiguration.Provider!;
                _endpointUrl = _request.AIModelEndpointConfiguration.Url!;

            }
            catch (OrchestrationException ex)
            {
                throw new SemanticKernelException(ex.Message, StatusCodes.Status400BadRequest);
            }

            await ExpandAndValidateAgent();

            return _llmProvider switch
            {
                APIEndpointProviders.MICROSOFT => await BuildResponseWithAzureOpenAI(),
                APIEndpointProviders.OPENAI => await BuildResponseWithOpenAI(),
                _ => throw new SemanticKernelException($"The LLM provider '{_llmProvider}' is not supported.")
            };
        }

        /// <summary>
        /// Builds the completion response using Azure OpenAI.
        /// </summary>
        /// <returns>A <see cref="LLMCompletionResponse"/> object containing the completion response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<LLMCompletionResponse> BuildResponseWithAzureOpenAI()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the completion response using OpenAI.
        /// </summary>
        /// <returns>A <see cref="LLMCompletionResponse"/> object containing the completion response.</returns>
        /// <exception cref="NotImplementedException"></exception>
        protected virtual async Task<LLMCompletionResponse> BuildResponseWithOpenAI()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the agent-specific details from the agent properties payload.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ExpandAndValidateAgent()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        protected async Task<string> GetConfigurationValue(string configName) =>
            (await GetResource<AppConfigurationKeyBase>(
                configName,
                ConfigurationResourceTypeNames.AppConfigurations,
                _configurationResourceProvider)).Value!;

        private async Task<T> GetResource<T>(string objectId, string resourceTypeName, IResourceProviderService resourceProviderService)
            where T : ResourceBase
        {
            var result = await resourceProviderService.HandleGetAsync(
                $"/{resourceTypeName}/{objectId.Split("/").Last()}", new UnifiedUserIdentity
                {
                    Name = "SemanticKernelAPI",
                    UserId = "SemanticKernelAPI",
                    Username = "SemanticKernelAPI"
                });
            var resource = (result as List<ResourceProviderGetResult<T>>)!.First().Resource;
            return resource;
        }
    }
}
