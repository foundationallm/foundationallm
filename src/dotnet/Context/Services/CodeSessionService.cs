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

            var codeSessionProviderResponse = await _codeSessionProviderService.CreateCodeSession(
                instanceId,
                codeSessionRequest.AgentName,
                codeSessionRequest.ConversationId,
                codeSessionRequest.Context,
                userIdentity);

            var codeSessionRecord = new ContextCodeSessionRecord(
                instanceId,
                codeSessionRequest.ConversationId,
                codeSessionProviderResponse.SessionId,
                CodeSessionProviderNames.AzureContainerAppsDynamicSessions,
                codeSessionProviderResponse.Endpoint,
                userIdentity);

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

            var codeSessionRecord = await _cosmosDBService.GetCodeSessionRecord(
                sessionId,
                userIdentity.UPN!)
                ?? throw new ContextServiceException(
                    $"The code session record with id {sessionId} was not found.",
                    StatusCodes.Status404NotFound);

            var uploadResults = request.FileNames.Distinct()
                .ToDictionary(x => x, x => false);

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
                FileUploadSuccess = uploadResults
            };
        }
    }
}
