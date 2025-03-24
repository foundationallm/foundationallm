using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Context.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM File service.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class FileService(
        IAzureCosmosDBFileService cosmosDBService,
        IStorageService storageService,
        ILogger<FileService> logger) : IFileService
    {
        private readonly IAzureCosmosDBFileService _cosmosDBService = cosmosDBService;
        private readonly IStorageService _storageService = storageService;
        private readonly ILogger<FileService> _logger = logger;

        /// <inheritdoc/>
        public async Task<ContextFileRecord> CreateFile(
            string instanceId,
            string conversationId,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity)
        {
            var fileRecord = new ContextFileRecord(
                instanceId,
                conversationId,
                fileName,
                contentType,
                content.Length,
                userIdentity);

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
        public async Task<Stream?> GetFileContent(
            string instanceId,
            string conversationId,
            string fileName,
            UnifiedUserIdentity userIdentity)
        {
            var fileRecords = await _cosmosDBService.GetFileRecords(
                conversationId,
                fileName,
                userIdentity.UPN!);

            if (fileRecords.Count == 0)
                return null;

            var fileRecord = fileRecords.First();

            var fileContent = await _storageService.ReadFileAsync(
                instanceId,
                fileRecord.FilePath,
                default);

            return fileContent.ToStream();
        }
    }
}
