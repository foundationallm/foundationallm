using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Context.Constants;
using FoundationaLLM.Context.Exceptions;
using FoundationaLLM.Context.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Implements the FoundationaLLM Code Session service.
    /// </summary>
    /// <param name="fileService">The file service used for file operations.</param>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="codeSessionProviderService">The code session provider service.</param>
    /// <param name="resourceValidatorFactory">The resource validator factory.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class CodeSessionService(
        IFileService fileService,
        IAzureCosmosDBCodeSessionService cosmosDBService,
        ICodeSessionProviderService codeSessionProviderService,
        IResourceValidatorFactory resourceValidatorFactory,
        ILogger<CodeSessionService> logger) : ICodeSessionService
    {
        IFileService _fileService = fileService;
        IAzureCosmosDBCodeSessionService _cosmosDBService = cosmosDBService;
        ICodeSessionProviderService _codeSessionProviderService = codeSessionProviderService;
        ILogger<CodeSessionService> _logger = logger;
        StandardValidator _validator = new(
            resourceValidatorFactory,
            message => new ContextServiceException(
                message,
                StatusCodes.Status400BadRequest));

        /// <inheritdoc/>
        public async Task<CreateCodeSessionResponse> CreateCodeSession(
            string instanceId,
            CreateCodeSessionRequest codeSessionRequest,
            UnifiedUserIdentity userIdentity)
        {
            await _validator.ValidateAndThrowAsync(codeSessionRequest);

            // Create a new code session using the code session provider service.
            var codeSessionProviderResponse = await _codeSessionProviderService.CreateCodeSession(
                instanceId,
                codeSessionRequest.AgentName,
                codeSessionRequest.ConversationId,
                codeSessionRequest.Context,
                userIdentity);

            // Create the attached code session record.
            var codeSessionRecord = new ContextCodeSessionRecord(
                instanceId,
                codeSessionRequest.ConversationId,
                codeSessionProviderResponse.SessionId,
                CodeSessionProviderNames.AzureContainerAppsDynamicSessions,
                codeSessionProviderResponse.Endpoint,
                userIdentity);

            // Save the code session record to the database (update if it already exists).
            await _cosmosDBService.UpsertCodeSessionRecord(codeSessionRecord);

            return codeSessionProviderResponse;
        }

        /// <inheritdoc/>
        public async Task<CodeSessionFileUploadResponse> UploadFilesToCodeSession(
            string instanceId,
            string sessionId,
            CodeSessionFileUploadRequest request,
            UnifiedUserIdentity userIdentity)
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

            var uploadResults = request.FileNames.Distinct()
                .ToDictionary(x => x, x => false);

            // Attempt to retrieve the file content for each file name and upload it to the code session.
            // Approach the upload in a best-effort manner, where we upload as many files as possible.
            // It is left to the caller to determine what to do if some files fail to upload.
            foreach (var fileName in uploadResults.Keys)
            {
                var fileContent = await _fileService.GetFileContent(
                    codeSessionRecord.InstanceId,
                    codeSessionRecord.ConversationId,
                    fileName,
                    userIdentity);

                uploadResults[fileName] =
                    fileContent != null
                    && await _codeSessionProviderService.UploadFileToCodeSession(
                        codeSessionRecord.Id,
                        codeSessionRecord.Endpoint,
                        fileName,
                        fileContent);
            }

            var codeSessionFileUpload = new ContextCodeSessionFileUploadRecord(
                instanceId,
                sessionId,
                uploadResults,
                userIdentity);

            await _cosmosDBService.UpsertCodeSessionFileUploadRecord(codeSessionFileUpload);

            return new CodeSessionFileUploadResponse
            {
                OperationId = codeSessionFileUpload.Id,
                FileUploadSuccess = uploadResults,
                AllFilesUploaded = uploadResults.Values.All(x => x)
            };
        }

        /// <inheritdoc/>
        public async Task<CodeSessionFileDownloadResponse> DownloadFilesFromCodeSession(
            string instanceId,
            string sessionId,
            string operationId,
            UnifiedUserIdentity userIdentity)
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

            var fileStoreItems = await _codeSessionProviderService.GetCodeSessionFileStoreItems(
                codeSessionRecord.Id,
                codeSessionRecord.Endpoint);

            var newFileStoreItems = fileStoreItems
                .Where(x =>
                    x.ParentPath != "/"
                    || !codeSessionFileUploadRecord.FileUploadSuccess.ContainsKey(x.Name)
                    || !codeSessionFileUploadRecord.FileUploadSuccess[x.Name]);

            var result = new CodeSessionFileDownloadResponse();

            foreach (var newFileStoreItem in newFileStoreItems)
            {
                var fileContentStream = await _codeSessionProviderService.DownloadFileFromCodeSession(
                    codeSessionRecord.Id,
                    codeSessionRecord.Endpoint,
                    newFileStoreItem.Name,
                    newFileStoreItem.ParentPath);

                if (fileContentStream == null)
                    result.Errors.Add($"{newFileStoreItem.ParentPath}/{newFileStoreItem.Name}");
                else
                {
                    var fileRecord = await _fileService.CreateFile(
                    codeSessionRecord.InstanceId,
                    ContextRecordOrigins.CodeSession,
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

                    result.FileRecords[fileRecord.Id] = fileRecord;
                }
            }

            return result;
        }
    }
}
