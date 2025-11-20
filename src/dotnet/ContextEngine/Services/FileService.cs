using FoundationaLLM.Common.Constants;
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
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using UtfUnknown;

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
        public async Task<ContextFileRecord> CreateFileForConversation(
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
                null,
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

            return fileRecord;
        }

        /// <inheritdoc/>
        public async Task<ContextFileRecord> CreateFileForAgent(
            string instanceId,
            string origin,
            string agentName,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata)
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
                fileRecord.FilePath.StartsWith("file/")
                    ? fileRecord.FilePath
                    : $"file/{fileRecord.FilePath}",
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
                fileRecord.FilePath.StartsWith("file/")
                    ? fileRecord.FilePath
                    : $"file/{fileRecord.FilePath}",
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

        /// <inheritdoc/>
        public async Task DeleteFileRecord(
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
