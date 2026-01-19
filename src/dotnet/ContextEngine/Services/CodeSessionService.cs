using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Services;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Context.Constants;
using FoundationaLLM.Context.Exceptions;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.ContextEngine.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Implements the FoundationaLLM Code Session service.
    /// </summary>
    /// <param name="fileService">The file service used for file operations.</param>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="codeSessionProviderServices">The code session provider services.</param>
    /// <param name="resourceValidatorFactory">The resource validator factory.</param>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class CodeSessionService(
        IFileService fileService,
        IAzureCosmosDBCodeSessionService cosmosDBService,
        IEnumerable<ICodeSessionProviderService> codeSessionProviderServices,
        IResourceValidatorFactory resourceValidatorFactory,
        IHttpClientFactory httpClientFactory,
        ILogger<CodeSessionService> logger) : ICodeSessionService
    {
        readonly IFileService _fileService = fileService;
        readonly IAzureCosmosDBCodeSessionService _cosmosDBService = cosmosDBService;
        readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        readonly ICodeSessionProviderService _codeInterpreterCodeSessionProviderService =
            codeSessionProviderServices.FirstOrDefault(x =>
                x.ProviderName == CodeSessionProviderNames.AzureContainerAppsCodeInterpreter)
                ?? throw new ContextServiceException(
                    $"The code session provider service {CodeSessionProviderNames.AzureContainerAppsCodeInterpreter} was not found.",
                    StatusCodes.Status500InternalServerError);

        ICodeSessionProviderService _customContainerCodeSessionProviderService =
            codeSessionProviderServices.FirstOrDefault(x =>
                x.ProviderName == CodeSessionProviderNames.AzureContainerAppsCustomContainer)
                ?? throw new ContextServiceException(
                    $"The code session provider service {CodeSessionProviderNames.AzureContainerAppsCustomContainer} was not found.",
                    StatusCodes.Status500InternalServerError);


        ILogger<CodeSessionService> _logger = logger;
        StandardValidator _validator = new(
            resourceValidatorFactory,
            message => new ContextServiceException(
                message,
                StatusCodes.Status400BadRequest));

        /// <inheritdoc/>
        public async Task<Result<CreateCodeSessionResponse>> CreateCodeSession(
            string instanceId,
            CreateCodeSessionRequest codeSessionRequest,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                await _validator.ValidateAndThrowAsync(
                    codeSessionRequest,
                    new Dictionary<string, object>
                    {
                        { CreateCodeSessionRequestValidator.UserPrincipalNameKey, userIdentity.UPN! }
                    });

                var codeSessionProviderService = GetCodeSessionProviderService(
                    codeSessionRequest.EndpointProvider,
                    codeSessionRequest.EndpointProviderOverride);

                // Create a new code session using the code session provider service.
                var codeSessionProviderResponse = await codeSessionProviderService.CreateCodeSession(
                    instanceId,
                    codeSessionRequest.AgentName,
                    codeSessionRequest.ConversationId,
                    codeSessionRequest.Context,
                    codeSessionRequest.Language,
                    userIdentity);

                // Create the attached code session record.
                var codeSessionRecord = new ContextCodeSessionRecord(
                    instanceId,
                    codeSessionRequest.ConversationId,
                    codeSessionProviderResponse.SessionId,
                    codeSessionRequest.EndpointProvider,
                    codeSessionProviderResponse.Endpoint,
                    codeSessionRequest.Language,
                    codeSessionRequest.EndpointProviderOverride,
                    userIdentity);

                // Save the code session record to the database (update if it already exists).
                await _cosmosDBService.UpsertCodeSessionRecord(codeSessionRecord);

                return Result<CreateCodeSessionResponse>.Success(codeSessionProviderResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating code session for instance {InstanceId} and conversation {ConversationId}.",
                    instanceId,
                    codeSessionRequest.ConversationId);
                return Result<CreateCodeSessionResponse>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<CodeSessionFileUploadResponse>> UploadFilesToCodeSession(
            string instanceId,
            string sessionId,
            CodeSessionFileUploadRequest request,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                await _validator.ValidateAndThrowAsync(request);

                // Identify the associated code session record.
                // If there is no record, we cannot move forward (the record was supposed to be created when the code session was created).
                var codeSessionRecord = await _cosmosDBService.GetCodeSessionRecord(
                    instanceId,
                    sessionId,
                    userIdentity.UPN!)
                    ?? throw new ContextServiceException(
                        $"The code session record with id {sessionId} was not found.",
                        StatusCodes.Status404NotFound);

                var codeSessionProviderService = GetCodeSessionProviderService(
                    codeSessionRecord.EndpointProvider,
                    codeSessionRecord.EndpointProviderOverride);

                var uploadResults = request.FileNames.Distinct()
                    .ToDictionary(x => x, x => false);

                //Attempt to cleanup the code session by removing any existing files.
                await codeSessionProviderService.DeleteCodeSessionFileStoreItems(
                    codeSessionRecord.Id,
                    codeSessionRecord.Endpoint);

                // Attempt to retrieve the file content for each file name and upload it to the code session.
                // Approach the upload in a best-effort manner, where we upload as many files as possible.
                // It is left to the caller to determine what to do if some files fail to upload.
                foreach (var fileName in uploadResults.Keys)
                {
                    var fileContentResult = await _fileService.GetFileContent(
                        codeSessionRecord.InstanceId,
                        codeSessionRecord.ConversationId,
                        fileName,
                        userIdentity);

                    uploadResults[fileName] =
                        fileContentResult.TryGetValue(out var fileContent)
                        && fileContent.FileContent != null
                        && await codeSessionProviderService.UploadFileToCodeSession(
                            codeSessionRecord.Id,
                            codeSessionRecord.Endpoint,
                            fileName,
                            fileContent.FileContent);
                }

                var codeSessionFileUpload = new ContextCodeSessionFileUploadRecord(
                    instanceId,
                    sessionId,
                    uploadResults,
                    userIdentity);

                await _cosmosDBService.UpsertCodeSessionFileUploadRecord(codeSessionFileUpload);

                return Result<CodeSessionFileUploadResponse>.Success(
                    new CodeSessionFileUploadResponse
                    {
                        OperationId = codeSessionFileUpload.Id,
                        FileUploadSuccess = uploadResults,
                        AllFilesUploaded = uploadResults.Values.All(x => x)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading files to code session {SessionId} for instance {InstanceId}.",
                    sessionId,
                    instanceId);
                return Result<CodeSessionFileUploadResponse>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<CodeSessionFileDownloadResponse>> DownloadFilesFromCodeSession(
            string instanceId,
            string sessionId,
            string operationId,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                // Identify the associated code session file upload record.
                var codeSessionFileUploadRecord = await _cosmosDBService.GetCodeSessionFileUploadRecord(
                    instanceId,
                    sessionId,
                    operationId,
                    userIdentity.UPN!)
                    ?? throw new ContextServiceException(
                        $"The code session file upload record with id {operationId} was not found.",
                        StatusCodes.Status404NotFound);

                var codeSessionRecord = await _cosmosDBService.GetCodeSessionRecord(
                    instanceId,
                    codeSessionFileUploadRecord.CodeSessionId,
                    userIdentity.UPN!)
                    ?? throw new ContextServiceException(
                        $"The code session record with id {codeSessionFileUploadRecord.CodeSessionId} was not found.",
                        StatusCodes.Status404NotFound);

                var codeSessionProviderService = GetCodeSessionProviderService(
                    codeSessionRecord.EndpointProvider,
                    codeSessionRecord.EndpointProviderOverride);

                var fileStoreItems = await codeSessionProviderService.GetCodeSessionFileStoreItems(
                    codeSessionRecord.Id,
                    codeSessionRecord.Endpoint);

                _logger.LogInformation("GetCodeSessionFileStoreItems result from {CodeSessionProviderName}: {CodeSessionProviderResult}",
                    codeSessionProviderService.ProviderName,
                    JsonSerializer.Serialize(fileStoreItems));

                var newFileStoreItems = fileStoreItems
                    .Where(x =>
                        x.ParentPath != "/"
                        || !codeSessionFileUploadRecord.FileUploadSuccess.ContainsKey(x.Name)
                        || !codeSessionFileUploadRecord.FileUploadSuccess[x.Name]);

                _logger.LogInformation("New file store items to download: {NewFileStoreItems}",
                    JsonSerializer.Serialize(newFileStoreItems));

                var result = new CodeSessionFileDownloadResponse();

                foreach (var newFileStoreItem in newFileStoreItems)
                {
                    var fileContentStream = await codeSessionProviderService.DownloadFileFromCodeSession(
                        codeSessionRecord.Id,
                        codeSessionRecord.Endpoint,
                        newFileStoreItem.Name,
                        newFileStoreItem.ParentPath);

                    if (fileContentStream == null)
                    {
                        _logger.LogError("DownloadFileFromCodeSession result from {CodeSessionProviderName} for {FileStoreItemName} in {FileStoreItemPath}: error.",
                            codeSessionProviderService.ProviderName,
                            newFileStoreItem.Name,
                            newFileStoreItem.ParentPath);
                        result.Errors.Add($"{newFileStoreItem.ParentPath}/{newFileStoreItem.Name}");
                    }
                    else
                    {
                        _logger.LogInformation("DownloadFileFromCodeSession result from {CodeSessionProviderName} for {FileStoreItemName} in {FileStoreItemPath}: success.",
                            codeSessionProviderService.ProviderName,
                            newFileStoreItem.Name,
                            newFileStoreItem.ParentPath);

                        var fileRecordResult = await _fileService.CreateFileForConversation(
                            codeSessionRecord.InstanceId,
                            ContextRecordOrigins.CodeSession,
                            null,
                            codeSessionRecord.ConversationId,
                            newFileStoreItem.Name,
                            newFileStoreItem.ContentType,
                            fileContentStream!,
                            userIdentity,
                            new Dictionary<string, string>()
                            {
                            {
                                ContextFileRecordMetadataPropertyNames.CodeSessionFileUploadRecordId,
                                codeSessionFileUploadRecord.Id
                            },
                            {
                                ContextFileRecordMetadataPropertyNames.CodeSessionFilePath,
                                newFileStoreItem.ParentPath
                            }
                            });

                        if (fileRecordResult.TryGetValue(out var fileRecord))
                            result.FileRecords[fileRecord.Id] = fileRecord;
                        else
                        {
                            _logger.LogError("CreateFileForConversation failed for file {FileStoreItemName} in {FileStoreItemPath}.",
                                newFileStoreItem.Name,
                                newFileStoreItem.ParentPath);
                            result.Errors.Add($"{newFileStoreItem.ParentPath}/{newFileStoreItem.Name}");
                        }
                    }
                }

                return Result<CodeSessionFileDownloadResponse>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading files from code session {SessionId} for instance {InstanceId}.",
                    sessionId,
                    instanceId);
                return Result<CodeSessionFileDownloadResponse>.FailureFromException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task<Result<CodeSessionCodeExecuteResponse>> ExecuteCodeInCodeSession(
            string instanceId,
            string sessionId,
            string codeToExecute,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var codeSessionRecord = await _cosmosDBService.GetCodeSessionRecord(
                    instanceId,
                    sessionId,
                    userIdentity.UPN!)
                    ?? throw new ContextServiceException(
                        $"The code session record with id {sessionId} was not found.",
                        StatusCodes.Status404NotFound);

                var codeSessionProviderService = GetCodeSessionProviderService(
                    codeSessionRecord.EndpointProvider,
                    codeSessionRecord.EndpointProviderOverride);

                var result = await codeSessionProviderService.ExecuteCodeInCodeSession(
                    codeSessionRecord.Id,
                    codeSessionRecord.Endpoint,
                    codeToExecute);

                return Result<CodeSessionCodeExecuteResponse>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing code in code session {SessionId} for instance {InstanceId}.",
                    sessionId,
                    instanceId);
                return Result<CodeSessionCodeExecuteResponse>.FailureFromException(ex);
            }
        }

        private ICodeSessionProviderService GetCodeSessionProviderService(
            string codeSessionProviderName,
            CodeSessionEndpointProviderOverride? codeSessionEndpointProviderOverride) =>
            codeSessionEndpointProviderOverride != null && codeSessionEndpointProviderOverride.Enabled
                ? new LocalCustomContainerService(
                    codeSessionEndpointProviderOverride.Endpoint,
                    _logger,
                    _httpClientFactory)
                : codeSessionProviderName switch
                    {
                        CodeSessionProviderNames.AzureContainerAppsCodeInterpreter =>
                            _codeInterpreterCodeSessionProviderService,
                        CodeSessionProviderNames.AzureContainerAppsCustomContainer =>
                            _customContainerCodeSessionProviderService,
                        _ => throw new ContextServiceException(
                            $"The code session provider service {codeSessionProviderName} is not supported.",
                                StatusCodes.Status400BadRequest)
                    };
    }
}
