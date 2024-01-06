using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Provides access to Azure Data Lake blob storage.
    /// </summary>
    public class DataLakeStorageService : IStorageService
    {
        BlobStorageServiceSettings _settings;
        ILogger<DataLakeStorageService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLakeStorageService"/> with the specified options and logger.
        /// </summary>
        /// <param name="options">The options object containing the <see cref="BlobStorageServiceSettings"/> object with the settings.</param>
        /// <param name="logger">The logger used for logging.</param>
        public DataLakeStorageService(
            IOptions<BlobStorageServiceSettings> options,
            ILogger<DataLakeStorageService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }
    }
}
