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
    public class FileService : IFileService
    {
        private readonly IAzureCosmosDBFileService _azureCosmosDBFileService;
        private readonly IStorageService _storageService;
        private readonly ILogger<FileService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
        /// <param name="logger">The logger used for logging.</param>
        public FileService(
            IAzureCosmosDBFileService azureCosmosDBFileService,
            IStorageService storageService,
            ILogger<FileService> logger)
        {
            _azureCosmosDBFileService = azureCosmosDBFileService;
            _storageService = storageService;
            _logger = logger;
        }

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

            fileRecord = await _azureCosmosDBFileService.UpsertItemAsync<ContextFileRecord>(
                fileRecord.UPN,
                fileRecord);

            await _storageService.WriteFileAsync(
                instanceId,
                fileRecord.FilePath,
                content,
                contentType,
                CancellationToken.None);

            return fileRecord;
        }
    }
}
