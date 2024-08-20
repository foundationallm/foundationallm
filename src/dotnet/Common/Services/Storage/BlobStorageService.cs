using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace FoundationaLLM.Common.Services.Storage
{
    /// <summary>
    /// Provides access to Azure blob storage.
    /// </summary>
    /// <remarks>
    ///  Initializes a new instance of the <see cref="BlobStorageService"/> with the specified options and logger.
    /// </remarks>
    /// <param name="storageOptions">The options object containing the <see cref="BlobStorageServiceSettings"/> object with the settings.</param>
    /// <param name="logger">The logger used for logging.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class BlobStorageService(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        IOptions<BlobStorageServiceSettings> storageOptions,
        ILogger<BlobStorageService> logger) : StorageServiceBase(storageOptions, logger), IStorageService
    {
        private BlobServiceClient _blobServiceClient;

        /// <summary>
        /// The name of the storage account.
        /// </summary>
        public string StorageAccountName => _blobServiceClient.AccountName;

        /// <inheritdoc/>
        public async Task<BinaryData> ReadFileAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            try
            {
                Response<BlobDownloadResult>? content = await blobClient.DownloadContentAsync(cancellationToken).ConfigureAwait(false);

                if (content != null && content.HasValue)
                {
                    return content.Value.Content;
                }

                throw new ContentException($"Cannot read file {filePath} from container {containerName}.");
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new ContentException("File not found.", e);
            }
        }

        /// <inheritdoc/>
        public async Task WriteFileAsync(
            string containerName,
            string filePath,
            Stream fileContent,
            string? contentType,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            var blobLeaseClient = blobClient.GetBlobLeaseClient();

            // We are using pessimistic conccurency by default.
            // For more details, see https://learn.microsoft.com/en-us/azure/storage/blobs/concurrency-manage.

            BlobLease? blobLease = default;

            try
            {
                if (await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    // We only need to get a lease for already existing blobs that are being updated.
                    blobLease = await blobLeaseClient.AcquireAsyncWithWait(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
                    if (blobLease == null)
                    {
                        _logger.LogError("Could not get a lease for the blob {FilePath} from container {ContainerName}. Reason: unkown.", filePath, containerName);
                        throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. Reason: unknown.");
                    }
                }

                fileContent.Seek(0, SeekOrigin.Begin);

                BlobUploadOptions options = new()
                {
                    HttpHeaders = new BlobHttpHeaders()
                    {
                        ContentType = string.IsNullOrWhiteSpace(contentType)
                            ? "application/json"
                            : contentType
                    },
                    Conditions = (blobLease != null)
                    ? new BlobRequestConditions()
                        {
                            LeaseId = blobLease!.LeaseId
                        }
                    : default
                };

                await blobClient.UploadAsync(fileContent, options, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == (int)HttpStatusCode.Conflict
                        && ex.ErrorCode == "LeaseAlreadyPresent")
                {
                    _logger.LogError(ex, "Could not get a lease for the blob {FilePath} from container {ContainerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.",
                        filePath, containerName);
                    throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.", ex);
                }

                throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. Reason: unknown.", ex);
            }
            finally
            {
                if (blobLease != null)
                    await blobLeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task WriteFileAsync(
            string containerName,
            string filePath,
            string fileContent,
            string? contentType,
            CancellationToken cancellationToken = default) =>
            await WriteFileAsync(
                containerName,
                filePath,
                new MemoryStream(Encoding.UTF8.GetBytes(fileContent)),
                contentType,
                cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task DeleteFileAsync(string containerName, string filePath,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            try
            {
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new ContentException("File not found.", e);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> FileExistsAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            return await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void CreateClientFromAccountKey(string accountName, string accountKey) =>
            _blobServiceClient = new BlobServiceClient(
                new Uri($"https://{accountName}.dfs.core.windows.net"),
                new StorageSharedKeyCredential(accountName, accountKey));

        /// <inheritdoc/>
        protected override void CreateClientFromConnectionString(string connectionString) =>
            _blobServiceClient = new BlobServiceClient(connectionString);

        /// <inheritdoc/>
        protected override void CreateClientFromIdentity(string accountName) =>
            _blobServiceClient = new BlobServiceClient(
                new Uri($"https://{accountName}.blob.core.windows.net"),
                DefaultAuthentication.AzureCredential);

        /// <inheritdoc/>
        public async Task<List<string>> GetFilePathsAsync(string containerName, string? directoryPath = null, bool recursive = true, CancellationToken cancellationToken = default)
        {
            var fullListing = new List<string>(); // Full listing of directory and file paths  
            var filePaths = new List<string>(); // List to store only file paths  
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (recursive)
            {
                // Flat listing (recursive)  
                await foreach (var blob in containerClient.GetBlobsAsync(prefix: directoryPath, cancellationToken: cancellationToken))
                {
                    if (!blob.Name.Equals(directoryPath))
                    {
                        fullListing.Add(blob.Name);
                    }                    
                }
                // Filter out subpaths, note that empty folders will not be filtered out
                // there is no way of knowing if an empty folder is a blob or a virtual directory
                filePaths = FilterSubpaths(fullListing);
            }
            else
            {
                // Hierarchical listing (non-recursive)  
                var prefix = string.IsNullOrEmpty(directoryPath) ? null : directoryPath.TrimEnd('/');

                if (!directoryPath.Contains("requests"))
                    directoryPath += "/";

                await foreach (var blobHierarchyItem in containerClient.GetBlobsByHierarchyAsync(delimiter: "/", prefix: prefix, cancellationToken: cancellationToken))
                {
                    if (blobHierarchyItem.IsBlob)
                    {
                        filePaths.Add(blobHierarchyItem.Blob.Name);
                    }
                    // Do not add if the item is a prefix (which represents a virtual directory)  
                }
            }

            return filePaths;
        }

        /// <summary>
        /// Removes subpaths (directories) from the list of paths.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns>List of file paths.</returns>
        private List<string> FilterSubpaths(List<string> paths)
        {          
            List<string> filteredPaths = new List<string>(paths);

            // Sort the list by length in descending order to ensure we always keep the longest strings  
            filteredPaths.Sort((a, b) => b.Length.CompareTo(a.Length));

            // Compare each path with all others to see if it's contained within any other path (indicative of directory)
            for (int i = 0; i < filteredPaths.Count; i++)
            {
                for (int j = i + 1; j < filteredPaths.Count; j++)
                {                    
                    if (filteredPaths[i].Contains(filteredPaths[j]))
                    {
                        filteredPaths.RemoveAt(j);                         
                        j--;
                    }
                }
            }
            return filteredPaths;
        }
    }
}
