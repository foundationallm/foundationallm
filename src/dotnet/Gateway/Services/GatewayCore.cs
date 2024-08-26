using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Azure;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;
using FoundationaLLM.Gateway.Models;
using FoundationaLLM.Gateway.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;
using System.Collections.Concurrent;
using System.Text.Json;

#pragma warning disable OPENAI001

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Implements the FoundationaLLM Gateway service.
    /// </summary>
    /// <param name="armService">The <see cref="IAzureResourceManagerService"/> instance providing Azure Resource Manager services.</param>
    /// <param name="options">The options providing the <see cref="GatewayCoreSettings"/> object.</param>
    /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    public class GatewayCore(
        IAzureResourceManagerService armService,
        IOptions<GatewayCoreSettings> options,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILoggerFactory loggerFactory) : IGatewayCore
    {
        private readonly IAzureResourceManagerService _armService = armService;
        private readonly GatewayCoreSettings _settings = options.Value;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly IResourceProviderService _attachmentResourceProvider =
            resourceProviderServices.Single(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Attachment);
        private readonly ILogger<GatewayCore> _logger = loggerFactory.CreateLogger<GatewayCore>();

        private bool _initialized = false;

        private Dictionary<string, AzureOpenAIAccount> _azureOpenAIAccounts = [];
        private Dictionary<string, EmbeddingModelContext> _embeddingModels = [];

        private ConcurrentDictionary<string, EmbeddingOperationContext> _embeddingOperations = [];

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("The Gateway core service is starting.");

            try
            {
                var openAIAccounts = _settings.AzureOpenAIAccounts.Split(";");

                foreach (var openAIAccount in openAIAccounts)
                {
                    _logger.LogInformation("Loading properties for the Azure OpenAI account with resource id {AccountResourceId}.", openAIAccount);

                    try
                    {
                        var accountProperties = await _armService.GetOpenAIAccountProperties(openAIAccount);
                        _azureOpenAIAccounts.Add(accountProperties.Name, accountProperties);

                        foreach (var deployment in accountProperties.Deployments)
                        {
                            if (deployment.CanDoEmbeddings)
                            {
                                var embeddingModelContext = new EmbeddingModelDeploymentContext(
                                    deployment,
                                    _loggerFactory);

                                if (!_embeddingModels.ContainsKey(deployment.ModelName))
                                    _embeddingModels[deployment.ModelName] = new EmbeddingModelContext(
                                        _embeddingOperations,
                                        _loggerFactory.CreateLogger<EmbeddingModelContext>())
                                    {
                                        ModelName = deployment.ModelName,
                                        DeploymentContexts = [embeddingModelContext]
                                    };
                                else
                                    _embeddingModels[deployment.ModelName].DeploymentContexts.Add(embeddingModelContext);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "There was an error while loading the properties for the Azure OpenAI account with resource id {AccountResourceId}.", openAIAccount);
                    }
                }

                _initialized = true;
                _logger.LogInformation("The Gateway core service started successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Gateway core did not start successfully due to an error.");
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("The Gateway core service is stopping.");
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            _logger.LogInformation("The Gateway core service is executing.");

            var modelTasks = _embeddingModels.Values
                .Select(em => Task.Run(() => em.ProcessOperations(cancellationToken)))
                .ToArray();

            await Task.WhenAll(modelTasks);
        }

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> StartEmbeddingOperation(string instanceId, TextEmbeddingRequest embeddingRequest, UnifiedUserIdentity userIdentity)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            if (!_embeddingModels.TryGetValue(embeddingRequest.EmbeddingModelName, out var embeddingModel))
                throw new GatewayException("The requested embedding model is not available.", StatusCodes.Status404NotFound);

            var operationId = Guid.NewGuid().ToString().ToLower();
            var embeddingOperationContext = new EmbeddingOperationContext
            {
                InputTextChunks = embeddingRequest.TextChunks.Select(tc => new TextChunk
                {
                    OperationId = operationId,
                    Position = tc.Position,
                    Content = tc.Content,
                    TokensCount = tc.TokensCount
                }).ToList(),
                Result = new TextEmbeddingResult
                {
                    InProgress = true,
                    OperationId = operationId,
                    TextChunks = embeddingRequest.TextChunks.Select(tc => new TextChunk
                    {
                        Position = tc.Position
                    }).ToList(),
                    TokenCount = 0
                }
            };

            embeddingModel.AddEmbeddingOperationContext(embeddingOperationContext);

            return await Task.FromResult(
                new TextEmbeddingResult
                {
                    InProgress = true,
                    OperationId = embeddingOperationContext.Result.OperationId
                });
        }

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingOperationResult(string instanceId, string operationId, UnifiedUserIdentity userIdentity)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            if (!_embeddingOperations.TryGetValue(operationId, out var operationContext))
                throw new GatewayException("The operation identifier was not found.", StatusCodes.Status404NotFound);

            if (operationContext.Result.Failed)
                return await Task.FromResult(new TextEmbeddingResult
                {
                    InProgress = false,
                    Failed = true,
                    ErrorMessage = operationContext.Result.ErrorMessage,
                    OperationId = operationId
                });
            else if (operationContext.Result.InProgress)
                return await Task.FromResult(new TextEmbeddingResult
                {
                    InProgress = true,
                    OperationId = operationId
                });
            else
                return await Task.FromResult(operationContext.Result);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> CreateAgentCapability(string instanceId, string capabilityCategory, string capabilityName, UnifiedUserIdentity userIdentity, Dictionary<string, object>? parameters = null)
        {
            if (!_initialized)
                throw new GatewayException("The Gateway service is not initialized.");

            return capabilityCategory switch
            {
                AgentCapabilityCategoryNames.OpenAIAssistants => await CreateOpenAIAgentCapability(instanceId, capabilityName, userIdentity, parameters!),
                _ => throw new GatewayException($"The agent capability category {capabilityCategory} is not supported by the Gateway service.",
                   StatusCodes.Status400BadRequest),
            };
        }

        private async Task<Dictionary<string, object>> CreateOpenAIAgentCapability(string instanceId, string capabilityName, UnifiedUserIdentity userIdentity, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(capabilityName))
                throw new GatewayException("The specified capability name is invalid.", StatusCodes.Status400BadRequest);

            Dictionary<string, object> result = [];
            var createAssistant = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.CreateAssistant, false);
            var createAssistantThread = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.CreateAssistantThread, false);
            var createAssistantFile = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.CreateAssistantFile, false);
            var addAssistantFileToVectorStore = GetParameterValue<bool>(parameters, OpenAIAgentCapabilityParameterNames.AddAssistantFileToVectorStore, false);

            var endpoint = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.Endpoint);
            var azureOpenAIAccount = _azureOpenAIAccounts.Values.FirstOrDefault(
                a => Uri.Compare(
                    new Uri(endpoint!),
                    new Uri(a.Endpoint),
                    UriComponents.Host,
                    UriFormat.SafeUnescaped,
                    StringComparison.OrdinalIgnoreCase) == 0)
                ?? throw new GatewayException($"The Gateway service is not configured to use the {endpoint} endpoint.");

            var azureOpenAIClient = new AzureOpenAIClient(new Uri(azureOpenAIAccount.Endpoint), DefaultAuthentication.AzureCredential);

            if (createAssistant)
            {
                var assistantClient = azureOpenAIClient.GetAssistantClient();

                var prompt = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.AssistantPrompt);
                var modelDeploymentName = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.ModelDeploymentName);
                var azureOpenAIModel = azureOpenAIAccount.Deployments.FirstOrDefault(
                    d => string.Compare(
                        modelDeploymentName,
                        d.Name,
                        true) == 0)
                    ?? throw new GatewayException($"The Gateway service cannot find the {modelDeploymentName} model deployment in the account with endpoint {endpoint}.");

                var assistantResult = await assistantClient.CreateAssistantAsync(modelDeploymentName, new AssistantCreationOptions()
                {
                    Name = capabilityName,
                    Instructions = prompt,
                    Tools =
                    {
                        new CodeInterpreterToolDefinition(),
                        new FileSearchToolDefinition()
                    }
                });

                var assistant = assistantResult.Value;
                result[OpenAIAgentCapabilityParameterNames.AssistantId] = assistant.Id;
            }

            if (createAssistantThread)
            {
                var assistantClient = azureOpenAIClient.GetAssistantClient();
                var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();

                var vectorStoreResult = await vectorStoreClient.CreateVectorStoreAsync(new VectorStoreCreationOptions
                {
                    ExpirationPolicy = new VectorStoreExpirationPolicy
                    {
                        Anchor = VectorStoreExpirationAnchor.LastActiveAt,
                        Days = 365
                    }
                });

                var threadResult = await assistantClient.CreateThreadAsync(new ThreadCreationOptions
                {
                    ToolResources = new ToolResources()
                    {
                        FileSearch = new FileSearchToolResources()
                        {
                            VectorStoreIds = [vectorStoreResult.Value.Id]
                        }
                    }
                });
                var thread = threadResult.Value;
                var vectorStore = vectorStoreResult.Value;

                result[OpenAIAgentCapabilityParameterNames.AssistantThreadId] = thread.Id;
                result[OpenAIAgentCapabilityParameterNames.AssistantVectorStoreId] = vectorStore.Id;
            }

            var fileId = GetParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.AssistantFileId, string.Empty);

            if (createAssistantFile)
            {
                var fileClient = azureOpenAIClient.GetFileClient();

                var attachmentObjectId = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.AttachmentObjectId);
                var attachmentFile = await _attachmentResourceProvider.GetResource<AttachmentFile>(attachmentObjectId, userIdentity, new ResourceProviderOptions { LoadContent = true });

                var fileResult = await fileClient.UploadFileAsync(
                    new MemoryStream(attachmentFile.Content!),
                    attachmentFile.OriginalFileName,
                    FileUploadPurpose.Assistants);
                var file = fileResult.Value;
                result[OpenAIAgentCapabilityParameterNames.AssistantFileId] = file.Id;
                fileId = file.Id;
            }

            if (addAssistantFileToVectorStore)
            {
                var vectorStoreClient = azureOpenAIClient.GetVectorStoreClient();
                var vectorStoreId = GetRequiredParameterValue<string>(parameters, OpenAIAgentCapabilityParameterNames.AssistantVectorStoreId);

                var vectorizationResult = await vectorStoreClient.AddFileToVectorStoreAsync(vectorStoreId, fileId);

                var startTime = DateTimeOffset.UtcNow;
                _logger.LogInformation("Started vectorization of file {FileId} in vector store {VectorStoreId}.", fileId, vectorStoreId);

                var pollingCount = 0;
                var maxPollingCountExceeded = false;
                while (vectorizationResult.Value.Status == VectorStoreFileAssociationStatus.InProgress)
                {
                    await Task.Delay(1000);
                    if (pollingCount++ > 1800)
                    {
                        // Will not wait more than 30 minutes for the vectorization to complete.
                        maxPollingCountExceeded = true;
                        break;
                    }
                    vectorizationResult = await vectorStoreClient.GetFileAssociationAsync(vectorStoreId, fileId);
                }

                if (maxPollingCountExceeded)
                    _logger.LogWarning("The maximum polling count was exceeded during the vectorization of file {FileId} in vector store {VectorStoreId}.", fileId, vectorStoreId);
                else
                    _logger.LogInformation("Completed vectorization of file {FileId} in vector store {VectorStoreId} in {TotalSeconds}.",
                        fileId, vectorStoreId, (DateTimeOffset.UtcNow - startTime).TotalSeconds);
            }

            return result;
        }

        private T GetParameterValue<T>(Dictionary<string, object> parameters, string parameterName, T defaultValue) =>
            parameters.TryGetValue(parameterName, out var parameterValueObject)
                ? ((JsonElement)parameterValueObject!).Deserialize<T>()
                    ?? defaultValue
                : defaultValue;

        private T GetRequiredParameterValue<T>(Dictionary<string, object> parameters, string parameterName) =>
            parameters.TryGetValue(parameterName, out var parameterValueObject)
                ? ((JsonElement)parameterValueObject!).Deserialize<T>()
                    ?? throw new GatewayException($"Could not load required parameter {parameterName}.", StatusCodes.Status400BadRequest)
                : throw new GatewayException($"The required parameter {parameterName} was not found.");
    }
}
