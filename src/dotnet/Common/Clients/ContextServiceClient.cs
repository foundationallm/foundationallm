using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Context;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides methods to call the FoundationaLLM Context API service.
    /// </summary>
    public class ContextServiceClient(
        ICallContext callContext,
        IHttpClientFactoryService httpClientFactoryService,
        ILogger<ContextServiceClient> logger) : IContextServiceClient
    {
        private readonly ICallContext _callContext = callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly ILogger<ContextServiceClient> _logger = logger;

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ContextFileRecord>> CreateFile(
            string instanceId,
            string conversationId,
            string fileName,
            string fileContentType,
            Stream fileContent)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var multipartFormDataContent = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileContent);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileContentType);
                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName
                };
                multipartFormDataContent.Add(streamContent);

                var responseMessage = await client.PostAsync(
                    $"instances/{instanceId}/conversations/{conversationId}/files",
                    multipartFormDataContent);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextFileRecord>(responseContent);

                    return response == null
                        ? new ContextServiceResponse<ContextFileRecord>
                        {
                            Success = false,
                            ErrorMessage = "An error occurred deserializing the response from the service."
                        }
                        : new ContextServiceResponse<ContextFileRecord>
                        {
                            Success = true,
                            Result = response
                        };
                }

                _logger.LogError(
                    "An error occurred while creating a file. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse<ContextFileRecord>
                {
                    Success = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a file.");
                return new ContextServiceResponse<ContextFileRecord>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating a file."
                };
            }
        }
    }
}
