using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides a base implementation for services that interact with code session containers, including file upload,
    /// download, execution, and management operations.
    /// </summary>
    /// <param name="logger">The logger used to record diagnostic and error information during container service operations. Cannot be null.</param>
    /// <param name="apiVersion">The API version to use.</param>
    public class CustomContainerServiceBase(
        ILogger logger,
        string apiVersion = "")
    {
        private readonly string _apiVersion = string.IsNullOrWhiteSpace(apiVersion)
            ? string.Empty
            : $"api-version={apiVersion}&";
        private readonly ILogger _logger = logger;

        /// <summary>
        /// Uploads a file to a code execution session.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>used to communicate with the code execution endpoint.</param>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="fileContent">The binary content of the file to upload.</param>
        /// <returns></returns>
        public async Task<bool> UploadFileToCodeSession(
            HttpClient httpClient,
            string codeSessionId,
            string endpoint,
            string fileName,
            BinaryData fileContent)
        {
            using var multipartFormDataContent = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileContent.ToStream());
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = fileName
            };
            multipartFormDataContent.Add(streamContent);

            var responseMessage = await httpClient.PostAsync(
                $"{endpoint}/files/upload?{_apiVersion}identifier={codeSessionId}",
                multipartFormDataContent);

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorContent = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogError("Failed to upload file {FileName} to code session {CodeSession}. Error: {Error}",
                    fileName, codeSessionId, errorContent);
            }

            return responseMessage.IsSuccessStatusCode;
        }

        /// <summary>
        /// Lists files from a code session.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>used to communicate with the code execution endpoint.</param>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <returns>The list of file paths from the code session.</returns>
        public async Task<List<CodeSessionFileStoreItem>> GetCodeSessionFileStoreItems(
            HttpClient httpClient,
            string codeSessionId,
            string endpoint)
        {
            var itemsToReturn = await GetCodeSessionFileStoreItems(
                codeSessionId,
                endpoint,
                httpClient);

            return itemsToReturn;
        }

        /// <summary>
        /// Deletes all files from a code session.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>used to communicate with the code execution endpoint.</param>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <returns></returns>
        public async Task DeleteCodeSessionFileStoreItems(
            HttpClient httpClient,
            string codeSessionId,
            string endpoint)
        {
            var url = $"{endpoint}/files/delete?{_apiVersion}identifier={codeSessionId}";
            var responseMessage = await httpClient.PostAsync(url, null);
            if (!responseMessage.IsSuccessStatusCode)
                _logger.LogError("Unable to delete the existing files from code session {CodeSession}.",
                    codeSessionId);
        }

        /// <summary>
        /// Downloads a file from a code session.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>used to communicate with the code execution endpoint.</param>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <param name="fileName">The name of the file to download.</param>
        /// <param name="filePath">The path to the file to download.</param>
        /// <returns>A stream with the binary content of the file.</returns>
        public async Task<Stream?> DownloadFileFromCodeSession(
            HttpClient httpClient,
            string codeSessionId,
            string endpoint,
            string fileName,
            string filePath)
        {
            var payload = new { file_name = fileName };
            using var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            var responseMessage = await httpClient.PostAsync(
                $"{endpoint}/files/download?{_apiVersion}identifier={codeSessionId}",
                content);

            return responseMessage.IsSuccessStatusCode
                ? responseMessage.Content.ReadAsStream()
                : null;
        }

        /// <summary>
        /// Executes code in a code session.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>used to communicate with the code execution endpoint.</param>
        /// <param name="codeSessionId">The identifier of the code session.</param>
        /// <param name="endpoint">The endpoint of the code session service.</param>
        /// <param name="codeToExecute">The code to execute.</param>
        /// <returns>The result of the code execution including standard and error outputs.</returns>
        public async Task<CodeSessionCodeExecuteResponse> ExecuteCodeInCodeSession(
            HttpClient httpClient,
            string codeSessionId,
            string endpoint,
            string codeToExecute)
        {
            try
            {
                var payload = new { code = codeToExecute };
                using var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(
                    $"{endpoint}/code/execute?{_apiVersion}identifier={codeSessionId}",
                    content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode
                    || string.IsNullOrWhiteSpace(responseContent))
                {
                    _logger.LogError(
                        "Code execution in session {CodeSessionId} returned an unsuccessful status code {ResponseStatus}. "
                        + "Raw response content: {ResponseContent}. "
                        + "Code to execute: {CodeToExecute}",
                        codeSessionId, response.StatusCode, responseContent, codeToExecute);
                    return new CodeSessionCodeExecuteResponse
                    {
                        Status = "Failed",
                        StandardOutput = string.Empty,
                        StandardError = "Empty response received from the code execution environment.",
                        ExecutionResult = "An unexpected response was received from the code execution sandbox. A common reason for this is the container running out of memory.",
                    };
                }

                try
                {
                    var responseJson = ((JsonElement)JsonSerializer.Deserialize<dynamic>(responseContent)).GetProperty("detail");

                    return new CodeSessionCodeExecuteResponse
                    {
                        Status = response.IsSuccessStatusCode
                        ? "Succeeded"
                        : "Failed",
                        StandardOutput = responseJson.GetProperty("output").ToString(),
                        StandardError = responseJson.GetProperty("error").ToString(),
                        ExecutionResult = responseJson.GetProperty("results").ToString(),
                    };
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Invalid JSON response received when executing code in code session {CodeSessionId}: {ResponseContent}",
                        codeSessionId, responseContent);
                    return new CodeSessionCodeExecuteResponse
                    {
                        Status = "Failed",
                        StandardOutput = string.Empty,
                        StandardError = "Invalid JSON response received from the code execution environment.",
                        ExecutionResult = "An unexpected response was received from the code execution sandbox. A common reason for this is the container timing out on execution.",
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing code in code session {CodeSessionId}", codeSessionId);
                return new CodeSessionCodeExecuteResponse
                {
                    Status = "Failed",
                    StandardOutput = string.Empty,
                    StandardError = ex.Message,
                    ExecutionResult = "The code execution failed due to a communication issue with the code execution environment.",
                };
            }
        }

        private async Task<List<CodeSessionFileStoreItem>> GetCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint,
            HttpClient httpClient,
            bool includeFolders = false,
            bool includeLocalPath = false)
        {
            var rootUrl = $"{endpoint}/files?{_apiVersion}identifier={codeSessionId}";
            var rootFileStore = await GetCodeSessionFileStore(
                httpClient,
                rootUrl,
                string.Empty);

            if (rootFileStore.Items.Count == 0)
                return [];

            var filesToReturn = rootFileStore.Items
                .Where(item => item.Type == "file")
                .ToList();

            var directoriesToReturn = new List<CodeSessionFileStoreItem>();

            var directoriesToProcess = rootFileStore.Items
                .Where(item => item.Type == "directory")
                .Select(x =>
                {
                    x.ParentPath = string.Empty;
                    return x;
                })
                .ToList();

            while (directoriesToProcess.Count > 0)
            {
                var directoryToProcess = directoriesToProcess.First();
                var fileStore = await GetCodeSessionFileStore(
                    httpClient,
                    rootUrl,
                    $"{directoryToProcess.ParentPath}/{directoryToProcess.Name}");

                if (includeFolders)
                    directoriesToReturn.Add(directoryToProcess);
                directoriesToProcess.RemoveAt(0);

                if (fileStore.Items.Count > 0)
                {
                    filesToReturn.AddRange(fileStore.Items.Where(item => item.Type == "file"));
                    directoriesToProcess.AddRange(fileStore.Items.Where(item => item.Type == "directory"));
                }
            }

            var result = includeFolders
                ? [.. filesToReturn, .. directoriesToReturn]
                : filesToReturn;

            return includeLocalPath
                ? [.. result.Select(x =>
                    {
                        x.ParentPath = $"/mnt/data{x.ParentPath}";
                        return x;
                    })]
                : result;
        }

        private async Task<CodeSessionFileStore> GetCodeSessionFileStore(
            HttpClient httpClient,
            string url,
            string path)
        {
            var urlWithPath = $"{url}{(string.IsNullOrWhiteSpace(path) ? string.Empty : $"&path={path}")}";
            var responseMessage = await httpClient.GetAsync(urlWithPath);

            if (!responseMessage.IsSuccessStatusCode)
            {
                return new();
            }
            else
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var fileStore = JsonSerializer.Deserialize<CodeSessionFileStore>(responseContent);
                foreach (var item in fileStore!.Items)
                    item.ParentPath = (string.IsNullOrWhiteSpace(path) && item.Type == "file")
                        ? "/"
                        : path;
                return fileStore!;
            }
        }
    }
}
