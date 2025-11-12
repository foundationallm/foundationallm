using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Tasks;
using FoundationaLLM.SemanticKernel.Core.Agents;
using FoundationaLLM.SemanticKernel.Core.Exceptions;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.SemanticKernel.Core.Services
{
    /// <summary>
    /// Processes requests targeting the Semantic Kernel agents.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to build loggers for logging.</param>
    /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
    /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
    /// <param name="resourceProviderServices">A collection of <see cref="IResourceProviderService"/> resource providers.</param>
    /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
    /// <param name="taskPool">The global <see cref="TaskPool"/> object that keeps track of active completion tasks.</param>
    public class SemanticKernelService(
        ILoggerFactory loggerFactory,
        IOrchestrationContext callContext,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IHttpClientFactoryService httpClientFactoryService,
        ConcurrentTaskPool taskPool) : ISemanticKernelService
    {
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly IEnumerable<IResourceProviderService> _resourceProviderServices = resourceProviderServices;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly ConcurrentTaskPool _taskPool = taskPool;

        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions(
            options =>
            {
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                return options;
            });

        /// <inheritdoc/>
        public async Task<LongRunningOperation> StartCompletionOperation(string instanceId, LLMCompletionRequest completionRequest)
        {
            var fallback = new LongRunningOperation
            {
                OperationId = completionRequest.OperationId,
                Status = OperationStatus.Failed,
                StatusMessage = "An error occured while attempting to start the completion operation."
            };

            var client = await _httpClientFactoryService.CreateClient(
                instanceId,
                HttpClientNames.StateAPI,
                _callContext.CurrentUserIdentity!);

            var response = await client.PostAsync($"instances/{instanceId}/operations/{completionRequest.OperationId}", null);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var operation = JsonSerializer.Deserialize<LongRunningOperation>(responseContent, _jsonSerializerOptions)!;

                var success = _taskPool.TryAdd([
                    new TaskInfo
                    {
                        PayloadId = operation.Id!,
                        StartTime = DateTime.UtcNow,
                        Task = RunCompletionOperation(instanceId, completionRequest, operation, client)
                    }]);

                if (!success)
                {
                    throw new SemanticKernelException(
                        "Could not schedule the execution of the completion operation on the internal task pool. This is most likely due to exceeding the maximum capacity of the task pool.",
                        StatusCodes.Status500InternalServerError);
                }

                return operation;
            }

            return fallback;
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId)
        {
            var fallback = new LongRunningOperation
            {
                OperationId = operationId,
                Status = OperationStatus.Failed
            };

            var client = await _httpClientFactoryService.CreateClient(
                instanceId,
                HttpClientNames.StateAPI,
                _callContext.CurrentUserIdentity!);

            var response = await client.GetAsync($"instances/{instanceId}/operations/{operationId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var operation = JsonSerializer.Deserialize<LongRunningOperation>(responseContent, _jsonSerializerOptions)!;

                return operation;
            }

            return fallback;
        }

        private async Task RunCompletionOperation(string instanceId, LLMCompletionRequest completionRequest, LongRunningOperation operation, HttpClient stateAPIClient)
        {
            operation.Status = OperationStatus.InProgress;
            operation.StatusMessage = "Operation state changed to in progress.";

            _ = await stateAPIClient.PutAsync(
                $"instances/{instanceId}/operations/{completionRequest.OperationId}",
                new StringContent(JsonSerializer.Serialize(operation, _jsonSerializerOptions), Encoding.UTF8, "application/json"));

            try
            {
                var completion = completionRequest.Agent switch
                {
                    GenericAgent => await (new SemanticKernelKnowledgeManagementAgent(
                        completionRequest,
                        _resourceProviderServices,
                        _loggerFactory,
                        _httpClientFactoryService)).GetCompletion(),
                    _ => throw new Exception($"The agent type {completionRequest.Agent.GetType()} is not supported.")
                };

                operation.Status = OperationStatus.Completed;
                operation.StatusMessage = $"Operation {completionRequest.OperationId} completed successfully.";

                _ = await stateAPIClient.PostAsync(
                    $"instances/{instanceId}/operations/{completionRequest.OperationId}/result",
                    new StringContent(JsonSerializer.Serialize(completion, _jsonSerializerOptions), Encoding.UTF8, "application/json"));

                _ = await stateAPIClient.PutAsync(
                    $"instances/{instanceId}/operations/{completionRequest.OperationId}",
                    new StringContent(JsonSerializer.Serialize(operation, _jsonSerializerOptions), Encoding.UTF8, "application/json")); 
            }
            catch (Exception ex)
            {
                operation.Status = OperationStatus.Failed;
                operation.StatusMessage = $"Operation failed with error: {ex}";

                _ = await stateAPIClient.PostAsync(
                    $"instances/{instanceId}/operations/{completionRequest.OperationId}/result",
                    new StringContent(JsonSerializer.Serialize(new LLMCompletionResponse()
                    {
                        OperationId = operation.OperationId!,
                        UserPrompt = completionRequest.UserPrompt,
                        Completion = $"Operation failed with error: {ex}"
                    }, _jsonSerializerOptions), Encoding.UTF8, "application/json"));

                _ = await stateAPIClient.PutAsync(
                    $"instances/{instanceId}/operations/{completionRequest.OperationId}",
                    new StringContent(JsonSerializer.Serialize(operation, _jsonSerializerOptions), Encoding.UTF8, "application/json"));
            }
        }
    }
}
