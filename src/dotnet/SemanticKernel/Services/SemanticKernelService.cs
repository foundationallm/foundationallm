﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.SemanticKernel.Core.Agents;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.SemanticKernel.Core.Services
{
    /// <summary>
    /// Processes requests targeting the Semantic Kernel agents.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to build loggers for logging.</param>
    /// <param name="resourceProviderServices">A collection of <see cref="IResourceProviderService"/> resource providers.</param>
    public class SemanticKernelService(
        ILoggerFactory loggerFactory,
        IEnumerable<IResourceProviderService> resourceProviderServices,        
        IHttpClientFactoryService httpClientFactoryService) : ISemanticKernelService
    {
        private readonly IEnumerable<IResourceProviderService> _resourceProviderServices = resourceProviderServices;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;

        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(LLMCompletionRequest request) => request.Agent switch
        {
            KnowledgeManagementAgent => await (new SemanticKernelKnowledgeManagementAgent(
                request,
                _resourceProviderServices,
                _loggerFactory,
                _httpClientFactoryService)).GetCompletion(),
            _ => throw new Exception($"The agent type {request.Agent.GetType()} is not supported.")
        };
}
}
