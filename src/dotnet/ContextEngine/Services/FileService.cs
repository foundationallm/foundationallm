using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Models.Services;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using UtfUnknown;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM File service.
    /// </summary>
    /// <param name="agentResourceProvider">The FoundationaLLM Agent resource provider.</param>
    /// <param name="dataPipelineResourceProvider">The FoundationaLLM Data Pipeline resource provider.</param>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="authorizationServiceClient">The client for the FoundationaLLM Authorization API.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class FileService(
        IResourceProviderService agentResourceProvider,
        IResourceProviderService dataPipelineResourceProvider,
        IAzureCosmosDBFileService cosmosDBService,
        IStorageService storageService,
        IAuthorizationServiceClient authorizationServiceClient,
        FileServiceSettings settings,
        ILogger<FileService> logger) : IFileService
    {
        private readonly IResourceProviderService _agentResourceProvider = agentResourceProvider;
        private readonly IResourceProviderService _dataPipelineResourceProvider = dataPipelineResourceProvider;
        private readonly IAzureCosmosDBFileService _cosmosDBService = cosmosDBService;
        private readonly IStorageService _storageService = storageService;
        private readonly IAuthorizationServiceClient _authorizationServiceClient = authorizationServiceClient;
        private readonly FileServiceSettings _settings = settings;
        private readonly HashSet<string> _knowledgeSearchFileTypes = [.. settings
            .KnowledgeSearchFileExtensions
            .Split(",")
            .Select(s => s.Trim().ToLower())];
        private readonly HashSet<string> _knowledgeSearchContextFileTypes = [.. settings
            .KnowledgeSearchContextFileExtensions?
            .Split(",")
            .Select(s => s.Trim().ToLower()) ?? []];
        private readonly Dictionary<string, int> _maxFileSizes =
            JsonSerializer.Deserialize<Dictionary<string, int>>(
                settings.KnowledgeSearchContextFileMaxSizeBytes ?? "{}")!
            .Select(x => x.Key
                .Split(",")
                .Select(y => new KeyValuePair<string, int>(
                    y.Trim().ToLower(),
                    x.Value)))
            .SelectMany(x => x)
            .ToDictionary();
        private readonly ILogger<FileService> _logger = logger;

        /// <inheritdoc/>
        public async Task<Result<ContextFileRecord>> CreateFileForConversation(
            string instanceId,
            string origin,
            string? agentName,
            string conversationId,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata)
        {
            try
            {
                var fileProcessingType = GetFileProcessingType(
                    origin,
                    Path.GetExtension(fileName).Replace(".", string.Empty).ToLower(),
                    content.Length);

                var fileRecord = new ContextFileRecord(
                    instanceId,
                    origin,
                    conversationId,
                    agentName,
                    fileName,
                    contentType,
                    content.Length,
                    fileProcessingType,
                    userIdentity,
                    metadata);

                await _cosmosDBService.UpsertFileRecord(fileRecord);

                if (ContentTypeMappings.TextContentTypes.Contains(contentType.ToLower()))
                    content = StandardizeTextContentEncoding(content);

                await _storageService.WriteFileAsync(
                    instanceId,
                    fileRecord.FilePath,
                    content,
                    contentType,
                    CancellationToken.None);

                var safetyBreachDetected = false;

                if (origin == ContextRecordOrigins.UserUpload)
                {
                    try
                    {
                        // Process the file as needed.
                        _logger.LogInformation("{InstanceId}: Processing uploaded file {FileName} ({FileId}) for conversation {ConversationId}.",
                            instanceId, fileName, fileRecord.Id, conversationId);

                        // First, if an agent is specified, try to get the data pipeline to execute from the agent's settings.
                        string? agentDataPipelineObjectId = null;

                        if (agentName is not null)
                        {
                            try
                            {
                                var agent = await _agentResourceProvider.GetResourceAsync<AgentBase>(
                                    instanceId,
                                    agentName!,
                                    userIdentity);

                                // If the agent has knowledge search settings, we will honor those settings.
                                var knowledgeSearchSettings = agent!.Tools
                                    .Select(t => t.GetKnowledgeSearchSettings())
                                    .Where(s => s != null)
                                    .SingleOrDefault();

                                if (knowledgeSearchSettings is not null)
                                {
                                    agentDataPipelineObjectId = knowledgeSearchSettings.FileUploadDataPipelineObjectId;
                                    _logger.LogInformation("{InstanceId}: While processing uploaded file {FileName} ({FileId}) for conversation {ConversationId}, agent {AgentName} requires the {DataPipelineObjectId} to be run.",
                                        instanceId, fileName, fileRecord.Id, conversationId, agentName, agentDataPipelineObjectId);
                                }
                                else
                                    _logger.LogInformation("{InstanceId}: While processing uploaded file {FileName} ({FileId}) for conversation {ConversationId}, agent {AgentName} does not have knowledge search settings.",
                                        instanceId, fileName, fileRecord.Id, conversationId, agentName);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "{InstanceId}: While processing uploaded file {FileName} ({FileId}) for conversation {ConversationId}, agent {AgentName} could not be loaded.",
                                    instanceId, fileName, fileRecord.Id, conversationId, agentName);
                            }
                        }

                        if (agentDataPipelineObjectId is not null
                            && fileProcessingType == FileProcessingTypes.ConversationDataPipeline)
                        {
                            _logger.LogInformation("{InstanceId}: File {FileName} ({FileId}) for conversation {ConversationId} will be processed using the {DataPipelineObjectId} data pipeline based on the {AgentName} agent settings.",
                                instanceId, fileName, fileRecord.Id, conversationId, agentDataPipelineObjectId, agentName);

                            // NOTE: In the current version, this processing will happen later in the flow (in the Orchestration API service).
                            // In future versions, we will consolidate the approach and trigger the data pipeline execution here.
                        }
                        else
                        {
                            var dataPipelineObjectId = ResourcePath.GetObjectId(
                                instanceId,
                                ResourceProviderNames.FoundationaLLM_DataPipeline,
                                DataPipelineResourceTypeNames.DataPipelines,
                                WellKnownDataPipelineNames.ShieldedFileContent);
                            _logger.LogInformation("{InstanceId}: File {FileName} ({FileId}) for conversation {ConversationId} will be processed using the {DataPipelineObjectId} data pipeline.",
                                instanceId, fileName, fileRecord.Id, conversationId, dataPipelineObjectId);

                            var newDataPipelineRun = DataPipelineRun.Create(
                                dataPipelineObjectId,
                                DataPipelineTriggerNames.DefaultManualTrigger,
                                new()
                                {
                                { DataPipelineTriggerParameterNames.DataSourceContextFileContextFileObjectId, fileRecord.FileObjectId},
                                { DataPipelineTriggerParameterNames.DataSourceContextFileContentAction, ContentItemActions.AddOrUpdate },
                                { DataPipelineTriggerParameterNames.StageExtractMaxContentSizeCharacters, 10_000_000 }
                                },
                                userIdentity.UPN!,
                                DataPipelineRunProcessors.Frontend);

                            _logger.LogInformation("{InstanceId}: File {FileName} ({FileId}) for conversation {ConversationId}: triggering {DataPipelineObjectId} data pipeline.",
                               instanceId, fileName, fileRecord.Id, conversationId, dataPipelineObjectId);

                            var dataPipelineSuccess = await PollingResourceRunner<DataPipelineRun>.Start(
                                instanceId,
                                _dataPipelineResourceProvider,
                                newDataPipelineRun,
                                TimeSpan.FromSeconds(1),
                                TimeSpan.FromSeconds(60),
                                _logger,
                                ServiceContext.ServiceIdentity!);
                            safetyBreachDetected = !dataPipelineSuccess;

                            _logger.LogInformation("{InstanceId}: File {FileName} ({FileId}) for conversation {ConversationId}: execution of {DataPipelineObjectId} data pipeline completed with (Success = {Success}).",
                               instanceId, fileName, fileRecord.Id, conversationId, dataPipelineObjectId, dataPipelineSuccess);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "{InstanceId}: There was an error processing the uploaded file {FileName} ({FileId}) for conversation {ConversationId}. The file upload will be canceled.",
                            instanceId, fileName, fileRecord.Id, conversationId);
                        safetyBreachDetected = true;
                    }
                }

                if (safetyBreachDetected)
                {
                    // Delete the file record and the file itself.
                    var deleteResult = await DeleteFileRecord(
                        instanceId,
                        fileRecord.Id,
                        userIdentity);

                    if (deleteResult.IsSuccess)
                        return Result<ContextFileRecord>.FailureFromErrorMessage(
                            $"The file {fileName} ({fileRecord.Id}) could not be uploaded for conversation {conversationId} because it did not pass the safety guardrails.",
                            StatusCodes.Status422UnprocessableEntity);
                    else
                    {
                        _logger.LogError("{InstanceId}: There was an error deleting the file record {FileId} after a safety breach was detected for file {FileName} for conversation {ConversationId}.",
                            instanceId, fileRecord.Id, fileName, conversationId);
                        return Result<ContextFileRecord>.FailureFromErrorMessage(
                            $"The file {fileName} ({fileRecord.Id}) could not be uploaded for conversation {conversationId} because it did not pass the safety guardrails. WARNING: There was an error deleting the file record.",
                            StatusCodes.Status422UnprocessableEntity);
                    }
                }
                else
                    return Result<ContextFileRecord>.Success(fileRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{InstanceId}: There was an error creating file {FileName} for conversation {ConversationId}.",
                    instanceId, fileName, conversationId);
                return Result<ContextFileRecord>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ContextFileRecord>> CreateFileForAgent(
            string instanceId,
            string origin,
            string agentName,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata)
        {
            try
            {
                var fileRecord = new ContextFileRecord(
                    instanceId,
                    origin,
                    null,
                    agentName,
                    fileName,
                    contentType,
                    content.Length,
                    GetFileProcessingType(
                        origin,
                        Path.GetExtension(fileName).Replace(".", string.Empty).ToLower(),
                        content.Length),
                    userIdentity,
                    metadata);

                await _cosmosDBService.UpsertFileRecord(fileRecord);

                if (ContentTypeMappings.TextContentTypes.Contains(contentType.ToLower()))
                    content = StandardizeTextContentEncoding(content);

                await _storageService.WriteFileAsync(
                    instanceId,
                    fileRecord.FilePath,
                    content,
                    contentType,
                    CancellationToken.None);

                return Result<ContextFileRecord>.Success(fileRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error in instance {InstanceId} creating file {FileName} for agent {AgentName}.",
                    instanceId, fileName, agentName);
                return Result<ContextFileRecord>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ContextFileContent>> GetFileContent(
            string instanceId,
            string conversationId,
            string fileName,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var bypassOwnerCheck = await ShouldBypassOwnerCheck(
                    instanceId, userIdentity);

                var fileRecords = await _cosmosDBService.GetFileRecords(
                    instanceId,
                    conversationId,
                    fileName,
                    userIdentity.UPN!,
                    bypassOwnerCheck);

                if (fileRecords.Count == 0)
                    return null;

                var fileRecord = fileRecords.First();

                var fileContent = await _storageService.ReadFileAsync(
                    instanceId,
                    fileRecord.FilePath.StartsWith("file/")
                        ? fileRecord.FilePath
                        : $"file/{fileRecord.FilePath}",
                    default);

                return Result<ContextFileContent>.Success(
                    new ContextFileContent
                    {
                        FileName = fileRecord.FileName,
                        ContentType = fileRecord.ContentType,
                        FileContent = fileContent.ToStream()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error in instance {InstanceId} retrieving file {FileName} for conversation {ConversationId}.",
                    instanceId, fileName, conversationId);
                return Result<ContextFileContent>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ContextFileContent>> GetFileContent(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var bypassOwnerCheck = await ShouldBypassOwnerCheck(
                    instanceId, userIdentity);

                var fileRecord = await _cosmosDBService.GetFileRecord(
                    instanceId,
                    fileId,
                    userIdentity.UPN!,
                    bypassOwnerCheck);

                var fileContent = await _storageService.ReadFileAsync(
                    instanceId,
                    fileRecord.FilePath.StartsWith("file/")
                        ? fileRecord.FilePath
                        : $"file/{fileRecord.FilePath}",
                    default);

                return Result<ContextFileContent>.Success(
                    new ContextFileContent
                    {
                        FileName = fileRecord.FileName,
                        ContentType = fileRecord.ContentType,
                        FileContent = fileContent.ToStream()
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error in instance {InstanceId} retrieving file {FileId}.",
                    instanceId, fileId);
                return Result<ContextFileContent>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<ContextFileRecord>> GetFileRecord(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var bypassOwnerCheck = await ShouldBypassOwnerCheck(
                    instanceId, userIdentity);

                var fileRecord = await _cosmosDBService.GetFileRecord(
                    instanceId,
                    fileId,
                    userIdentity.UPN!,
                    bypassOwnerCheck);

                return Result<ContextFileRecord>.Success(fileRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error in instance {InstanceId} retrieving file record {FileId}.",
                    instanceId, fileId);
                return Result<ContextFileRecord>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result> DeleteFileRecord(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var bypassOwnerCheck = await ShouldBypassOwnerCheck(
                    instanceId, userIdentity);

                var fileRecord = await _cosmosDBService.GetFileRecord(
                    instanceId,
                    fileId,
                    userIdentity.UPN!,
                    bypassOwnerCheck);

                await _storageService.DeleteFileAsync(
                    instanceId,
                    fileRecord.FilePath.StartsWith("file/")
                        ? fileRecord.FilePath
                        : $"file/{fileRecord.FilePath}",
                    CancellationToken.None);

                // Bypass owner check is not needed here as we have already retrieved the file record above.
                // Cover both cases (bypassOwnerCheck = True/False) we are sending the UPN from the file record.
                // In the case of bypassOwnerCheck = True, we've already validated the file record can be deleted
                // even if the calling user is not the owner of the file record. We need to send in the correct
                // UPN to allow the more efficient point delete operation in Cosmos DB.
                await _cosmosDBService.DeleteFileRecord(
                    instanceId,
                    fileId,
                    fileRecord.UPN);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error in instance {InstanceId} deleting file record {FileId}.",
                    instanceId, fileId);
                return Result.FailureFromException(ex);
            }
        }

        private async Task<bool> ShouldBypassOwnerCheck(
            string instanceId,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                // We are interested to check if the user has the Data Pipelines Execution Manager role.
                // The authorizable action is only provided for compliance with the validation requirements for the
                // action authorization request (setting it to null or an empty string will not work).
                // Same applies to the resource path, the one we provide does not exist.

                var resourcePath = ResourcePath.GetObjectId(
                    instanceId,
                    ResourceProviderNames.FoundationaLLM_DataPipeline,
                    DataPipelineResourceTypeNames.DataPipelines,
                    WellKnownResourceIdentifiers.NewResource);

                var authorizationResult = await _authorizationServiceClient
                    .ProcessAuthorizationRequest(
                        instanceId,
                        AuthorizableActionNames.FoundationaLLM_DataPipeline_DataPipelines_Read,
                        RoleDefinitionNames.Data_Pipelines_Execution_Manager + "!", // ! indicates the optional role assignment must be checked.
                        [resourcePath],
                        false, false, false,
                        userIdentity);

                return authorizationResult
                    .AuthorizationResults[resourcePath]
                    .HasRequiredRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error checking if ownership check should be bypassed for user {UserPrincipalName}.",
                    userIdentity.UPN!);
                return false;
            }
        }

        private string GetFileProcessingType(
            string fileOrigin,
            string fileExtension,
            long fileSizeBytes) =>
            fileOrigin switch
            {
                ContextRecordOrigins.CodeSession => FileProcessingTypes.None,
                ContextRecordOrigins.UserUpload =>
                    _knowledgeSearchFileTypes.Contains(fileExtension)
                        ? (
                            _knowledgeSearchContextFileTypes.Contains(fileExtension)
                            && _maxFileSizes.TryGetValue(fileExtension, out var maxSize)
                            && fileSizeBytes <= maxSize
                                ? FileProcessingTypes.CompletionRequestContext
                                : FileProcessingTypes.ConversationDataPipeline
                        )
                        : FileProcessingTypes.None,
                _ => FileProcessingTypes.None
            };

        private Stream StandardizeTextContentEncoding(
            Stream content)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (content.CanSeek)
                content.Seek(0, SeekOrigin.Begin);

            // Ensure legacy code pages (Windows-125x, etc.) are available on .NET Core/5+/6+/7+/8+
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Detect
            var result = CharsetDetector.DetectFromStream(content);
            var detectedEncoding = result.Detected; // may be null if uncertain
            var sourceEncoding = detectedEncoding?.Encoding ?? Encoding.GetEncoding(1252); // sensible fallback

            if (content.CanSeek)
                content.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(content, sourceEncoding, detectEncodingFromByteOrderMarks: true);

            // Convert to UTF-8
            string text = streamReader.ReadToEnd();
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
            return new MemoryStream(utf8Bytes);
        }
    }
}
