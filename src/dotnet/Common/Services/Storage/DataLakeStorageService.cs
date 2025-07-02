using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.Storage
{
    /// <summary>
    /// Provides access to Azure Data Lake blob storage.
    /// </summary>
    public class DataLakeStorageService : StorageServiceBase, IStorageService
    {
        private DataLakeServiceClient _dataLakeClient;

        /// <summary>
        /// The name of the storage account.
        /// </summary>
        public string StorageAccountName => _dataLakeClient.AccountName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLakeStorageService"/> with the specified options and logger.
        /// </summary>
        /// <param name="options">The options object containing the <see cref="BlobStorageServiceSettings"/> object with the blob storage settings.</param>
        /// <param name="logger">The logger used for logging.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DataLakeStorageService(
            IOptions<BlobStorageServiceSettings> options,
            ILogger<DataLakeStorageService> logger) : base(options, logger)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataLakeStorageService"/> with the specified options and logger.
        /// </summary>
        /// <param name="storageSettings">The <see cref="BlobStorageServiceSettings"/> object with the blob storage settings.</param>
        /// <param name="logger">The logger used for logging.</param>
        public DataLakeStorageService(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            BlobStorageServiceSettings storageSettings,
            ILogger<DataLakeStorageService> logger) : base(storageSettings, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetFilePathsAsync(
                       string containerName,
                       string? directoryPath = null,
                       bool recursive = true,
                       CancellationToken cancellationToken = default)
        {
            List<string> retValue = new List<string>();
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var filePaths = fileSystemClient.GetPathsAsync(path: directoryPath, recursive: recursive, cancellationToken: cancellationToken);
            await foreach (PathItem pathItem in filePaths)
            {
                if (pathItem.IsDirectory!.Value)
                    continue;

                retValue.Add(pathItem.Name);
            }
            return retValue;
        }

        /// <inheritdoc/>
        public async Task<BinaryData> ReadFileAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);

            try
            {
                var memoryStream = new MemoryStream();
                var result = await fileClient.ReadToAsync(memoryStream, null, cancellationToken).ConfigureAwait(false);

                if (result.IsError)
                    throw new ContentException($"Cannot read file {filePath} from file system {containerName}.");

                memoryStream.Seek(0, SeekOrigin.Begin);
                return BinaryData.FromStream(memoryStream);
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new ContentException("File not found.", e);
            }
        }

        /// <inheritdoc/>
        public BinaryData ReadFile(
            string containerName,
            string filePath)
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);

            try
            {
                var memoryStream = new MemoryStream();
                var result = fileClient.ReadTo(memoryStream, null);

                if (result.IsError)
                    throw new ContentException($"Cannot read file {filePath} from file system {containerName}.");

                memoryStream.Seek(0, SeekOrigin.Begin);
                return BinaryData.FromStream(memoryStream);
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
            CancellationToken cancellationToken)
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);
            var fileLeaseClient = fileClient.GetDataLakeLeaseClient();

            // We are using pessimistic conccurency by default.
            // For more details, see https://learn.microsoft.com/en-us/azure/storage/blobs/concurrency-manage.

            DataLakeLease? fileLease = default;

            try
            {
                if (await fileClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    // We only need to get a lease for already existing blobs that are being updated.
                    fileLease = await fileLeaseClient.AcquireAsync(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
                    if (fileLease == null)
                    {
                        _logger.LogError("Could not get a lease for the file {FilePath} from container {ContainerName}. Reason: unkown.", filePath, containerName);
                        throw new StorageException($"Could not get a lease for the file {filePath} from container {containerName}. Reason: unknown.");
                    }
                }

                fileContent.Seek(0, SeekOrigin.Begin);

                DataLakeFileUploadOptions options = new()
                {
                    HttpHeaders = new PathHttpHeaders()
                    {
                        ContentType = string.IsNullOrWhiteSpace(contentType)
                            ? "application/json"
                            : contentType
                    },
                    Conditions = (fileLease != null)
                    ? new DataLakeRequestConditions()
                    {
                        LeaseId = fileLease!.LeaseId
                    }
                    : default
                };

                await fileClient.UploadAsync(fileContent, options, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == (int)HttpStatusCode.Conflict
                        && ex.ErrorCode == "LeaseAlreadyPresent")
                {
                    _logger.LogError(ex, "Could not get a lease for the file {FilePath} from container {ContainerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.",
                        filePath, containerName);
                    throw new StorageException($"Could not get a lease for the file {filePath} from container {containerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.", ex);
                }

                throw new StorageException($"Could not get a lease for the file {filePath} from container {containerName}. Reason: unknown.", ex);
            }
            finally
            {
                if (fileLease != null)
                    await fileLeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task WriteFileAsync(
            string containerName,
            string filePath,
            string fileContent,
            string? contentType,
            CancellationToken cancellationToken) =>
            await WriteFileAsync(
                containerName,
                filePath,
                new MemoryStream(Encoding.UTF8.GetBytes(fileContent)),
                contentType,
                cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task UpdateJSONFileAsync<T>(
            string containerName,
            string filePath,
            Func<T, T> objectTransformer,
            CancellationToken cancellationToken) where T : class
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);
            var fileLeaseClient = fileClient.GetDataLakeLeaseClient();

            // We are using pessimistic conccurency by default.
            // For more details, see https://learn.microsoft.com/en-us/azure/storage/blobs/concurrency-manage.

            DataLakeLease? fileLease = default;

            try
            {
                fileLease = await fileLeaseClient.AcquireAsync(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
                if (fileLease == null)
                {
                    _logger.LogError("Could not get a lease for the file {FilePath} from container {ContainerName}. Reason: unkown.", filePath, containerName);
                    throw new StorageException($"Could not get a lease for the file {filePath} from container {containerName}. Reason: unknown.");
                }

                var memoryStream = new MemoryStream();
                var result = await fileClient.ReadToAsync(memoryStream, null, cancellationToken).ConfigureAwait(false);

                if (result.IsError)
                    throw new StorageException($"Cannot read file {filePath} from file system {containerName}.");

                memoryStream.Seek(0, SeekOrigin.Begin);
                var fileContent = default(T);

                if (JsonSerializer.Deserialize(BinaryData.FromStream(memoryStream), typeof(T)) is T content)
                {
                    fileContent = objectTransformer(content);
                }
                else
                {
                    _logger.LogError("Could not deserialize the content of the blob {FilePath} from container {ContainerName} to type {TypeName}.",
                        filePath, containerName, nameof(T));
                    throw new StorageException(
                        $"Could not deserialize the content of the blob {filePath} from container {containerName} to type {nameof(T)}.");
                }

                DataLakeFileUploadOptions options = new()
                {
                    HttpHeaders = new PathHttpHeaders()
                    {
                        ContentType = "application/json"
                    },
                    Conditions = (fileLease != null)
                    ? new DataLakeRequestConditions()
                    {
                        LeaseId = fileLease!.LeaseId
                    }
                    : default
                };

                await fileClient.UploadAsync(
                    new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(fileContent))),
                    options, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == (int)HttpStatusCode.Conflict
                        && ex.ErrorCode == "LeaseAlreadyPresent")
                {
                    _logger.LogError(ex, "Could not get a lease for the file {FilePath} from container {ContainerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.",
                        filePath, containerName);
                    throw new StorageException($"Could not get a lease for the file {filePath} from container {containerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.", ex);
                }

                throw new StorageException($"Could not get a lease for the file {filePath} from container {containerName}. Reason: unknown.", ex);
            }
            finally
            {
                if (fileLease != null)
                    await fileLeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFileAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);

            try
            {
                await fileClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
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
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);

            return await fileClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void CreateClientFromAccountKey(string accountName, string accountKey) =>
            _dataLakeClient = new DataLakeServiceClient(
                BuildStorageEndpointUri(accountName),
                new StorageSharedKeyCredential(accountName, accountKey));

        /// <inheritdoc/>
        protected override void CreateClientFromConnectionString(string connectionString) =>
            _dataLakeClient = new DataLakeServiceClient(connectionString);

        /// <inheritdoc/>
        protected override void CreateClientFromIdentity(string accountName) =>
            _dataLakeClient = new DataLakeServiceClient(
                BuildStorageEndpointUri(accountName),
                ServiceContext.AzureCredential);

        /// <summary>
        /// Builds the endpoint for the Azure Data Lake service.
        /// </summary>
        /// <param name="accountName">Name of the storage account</param>
        /// <returns>Uri of the storage account</returns>
        private Uri BuildStorageEndpointUri(string accountName)
        {
            // The account name "onelake" is used when using Microft Fabric
            // ref: https://learn.microsoft.com/en-us/fabric/onelake/onelake-access-api#uri-syntax
            if (accountName.ToLower().Equals(StorageNames.OneLake_Storage_Account))
            {
                return new Uri($"https://{StorageNames.OneLake_Storage_Account}.dfs.fabric.microsoft.com");
            }
            return new Uri($"https://{accountName}.dfs.core.windows.net");
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetMatchingFilePathsAsync(
            string containerName,
            string filePathPattern,
            CancellationToken cancellationToken = default)
        {
            var fullListing = new List<string>(); // Full listing of directory and file paths  
            var containerClient = _dataLakeClient.GetFileSystemClient(containerName);

            // Flat listing (recursive)  
            await foreach (var blob in containerClient.GetPathsAsync(path: filePathPattern, cancellationToken: cancellationToken))
            {
                fullListing.Add(blob.Name);
            }

            return fullListing;
        }

        /// <inheritdoc/>
        public async Task CopyFileAsync(
            string containerName,
            string sourceFilePath,
            string destinationFilePath,
            CancellationToken cancellationToken = default)
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var sourceFileClient = fileSystemClient.GetFileClient(sourceFilePath);
            var destinationFileClient = fileSystemClient.GetFileClient(destinationFilePath);

            using Stream sourceStream = await sourceFileClient.OpenReadAsync(
                cancellationToken: cancellationToken).ConfigureAwait(false);
            using Stream destinationStream = await destinationFileClient.OpenWriteAsync(
                overwrite: true,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(false);
        }
    }
}
