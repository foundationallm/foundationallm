using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM File service.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="authorizationServiceClient">The client for the FoundationaLLM Authorization API.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class FileService(
        IAzureCosmosDBFileService cosmosDBService,
        IStorageService storageService,
        IAuthorizationServiceClient authorizationServiceClient,
        FileServiceSettings settings,
        ILogger<FileService> logger) : IFileService
    {
        private readonly IAzureCosmosDBFileService _cosmosDBService = cosmosDBService;
        private readonly IStorageService _storageService = storageService;
        private readonly IAuthorizationServiceClient _authorizationServiceClient = authorizationServiceClient;
        private readonly FileServiceSettings _settings = settings;
        private readonly HashSet<string> _knowledgeSearchFileTypes = [.. settings
            .KnowledgeSearchFileExtensions
            .Split(",")
            .Select(s => s.Trim().ToLower())];
        private readonly ILogger<FileService> _logger = logger;

        /// <inheritdoc/>
        public async Task<ContextFileRecord> CreateFile(
            string instanceId,
            string origin,
            string conversationId,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata)
        {
            var fileRecord = new ContextFileRecord(
                instanceId,
                origin,
                conversationId,
                fileName,
                contentType,
                content.Length,
                origin switch
                {
                    ContextRecordOrigins.CodeSession => FileProcessingTypes.None,
                    ContextRecordOrigins.UserUpload => _knowledgeSearchFileTypes
                        .Contains(Path.GetExtension(fileName).Replace(".", string.Empty).ToLower())
                            ? FileProcessingTypes.ConversationDataPipeline
                            : FileProcessingTypes.None,
                    _ => FileProcessingTypes.None
                },
                userIdentity,
                metadata);

            await _cosmosDBService.UpsertFileRecord(fileRecord);

            await _storageService.WriteFileAsync(
                instanceId,
                fileRecord.FilePath,
                content,
                contentType,
                CancellationToken.None);

            return fileRecord;
        }

        /// <inheritdoc/>
        public async Task<ContextFileContent?> GetFileContent(
            string instanceId,
            string conversationId,
            string fileName,
            UnifiedUserIdentity userIdentity)
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
                fileRecord.FilePath,
                default);

            return new ContextFileContent
            {
                FileName = fileRecord.FileName,
                ContentType = fileRecord.ContentType,
                FileContent = fileContent.ToStream()
            };
        }

        /// <inheritdoc/>
        public async Task<ContextFileContent?> GetFileContent(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity)
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
                fileRecord.FilePath,
                default);

            return new ContextFileContent
            {
                FileName = fileRecord.FileName,
                ContentType = fileRecord.ContentType,
                FileContent = fileContent.ToStream()
            };
        }

        /// <inheritdoc/>
        public async Task<ContextFileRecord?> GetFileRecord(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            var bypassOwnerCheck = await ShouldBypassOwnerCheck(
                instanceId, userIdentity);

            var fileRecord = await _cosmosDBService.GetFileRecord(
                instanceId,
                fileId,
                userIdentity.UPN!,
                bypassOwnerCheck);

            return fileRecord;
        }

        public async Task<bool> ShouldBypassOwnerCheck(
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
                    "new");

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
    }
}
