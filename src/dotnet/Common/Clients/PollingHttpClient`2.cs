using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides a generic HTTP client that can be used to poll for a response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the payload to send to start the operation.</typeparam>
    /// <typeparam name="TResponse">The type of the response received when the operation is completed.</typeparam>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the requests.</param>
    /// <param name="request">The <typeparamref name="TRequest"/> request to send to the service.</param>
    /// <param name="operationStarterPath">The path to send the request to (will be appended to the base path of the HTTP client.</param>
    /// <param name="pollingInterval">The <see cref="TimeSpan"/> interval to poll for the response.</param>
    /// <param name="maxWaitTime">The <see cref="TimeSpan"/> maximum time to wait for the response.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class PollingHttpClient<TRequest, TResponse>(
        HttpClient httpClient,
        TRequest request,
        string operationStarterPath,
        TimeSpan pollingInterval,
        TimeSpan maxWaitTime,
        ILogger logger)
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly TRequest _request = request;
        private readonly string _operationStarterPath = operationStarterPath;
        private readonly TimeSpan _pollingInterval = pollingInterval;
        private readonly TimeSpan _maxWaitTime = maxWaitTime;
        private readonly ILogger _logger = logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions(
            options => {
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                return options;
            });

        /// <summary>
        /// <para>Retrieves the response from the service using a polling mechanism.
        /// The polling mechanism is based on the following assumptions:</para>
        /// - The {operationStarterPath} endpoint will accept a POST with a <typeparamref name="TRequest"/> object as payload and will return a 202 Accepted status code when the operation is started.<br/>
        /// - The returned response will contain a LongRunningOperation object with the operation id.<br/>
        /// - The polling endpoint is available at {operationStarterPath}/{operationId}/status.<br/>
        /// - The polling endpoint will return a 200 status code when the operation is in progress containing a property for status.<br/>
        /// - The polling endpoint will return a 200 status code when the operation is completed with the status property indicating Completed.<br/>
        /// - The result retrieval endpoint is available at {operationStarterPath}/{operationId}/result.<br/>
        /// - The polling endpoint will return a 404 Not Found status code when the operation is not found.<br/>
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> indicating the need to cancel the process.</param>
        /// <returns>The <typeparamref name="TResponse"/> object containing the response.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<TResponse?> GetResponseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var body = JsonSerializer.Serialize(_request, _jsonSerializerOptions);
                var responseMessage = await _httpClient.PostAsync(
                    _operationStarterPath,
                    new StringContent(
                        body,
                        Encoding.UTF8, "application/json"),
                    cancellationToken);

                if (responseMessage.StatusCode != HttpStatusCode.Accepted)
                {
                    _logger.LogError("The operation could not be started. The response status code was {StatusCode}.", responseMessage.StatusCode);
                    return default;
                }

                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var runningOperation = JsonSerializer.Deserialize<LongRunningOperation>(responseContent, _jsonSerializerOptions)!;

                _logger.LogInformation("The operation was started successfully. The operation id is {OperationId}.", runningOperation.OperationId);
                var operationStatusPath = $"{_operationStarterPath}/{runningOperation.OperationId}/status";
                var operationResultPath = $"{_operationStarterPath}/{runningOperation.OperationId}/result";

                var pollingStartTime = DateTime.UtcNow;
                var pollingCounter = 0;
                var currentPollingInterval = _pollingInterval;

                while (true)
                {
                    var totalPollingTime = DateTime.UtcNow - pollingStartTime;
                    pollingCounter++;
                    _logger.LogInformation(
                        "Polling for operation id {Operationid} (counter: {PollingCounter}, time elapsed: {PollingSeconds} seconds)...",
                        runningOperation.OperationId,
                        pollingCounter,
                        (int)totalPollingTime.TotalSeconds);

                    responseMessage = await _httpClient.GetAsync(
                        operationStatusPath,
                        cancellationToken);

                    if(responseMessage.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogError("The operation was not found. The operation id was {OperationId}.", runningOperation.OperationId);
                        return default;
                    }
                    if(responseMessage.StatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("An error occurred while polling for the response. The response status code was {StatusCode}.", responseMessage.StatusCode);
                        return default;
                    }

                    //status code is 200/OK
                    responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var currentStatus = JsonSerializer.Deserialize<LongRunningOperation>(responseContent, _jsonSerializerOptions)!;

                    switch(currentStatus.Status)
                    {
                        case OperationStatus.Completed:
                            var resultResponse = await _httpClient.GetAsync(operationResultPath, cancellationToken);
                            var resultContent = await resultResponse.Content.ReadAsStringAsync();
                            return JsonSerializer.Deserialize<TResponse>(resultContent, _jsonSerializerOptions);
                        case OperationStatus.InProgress:
                            if (totalPollingTime > _maxWaitTime)
                            {
                                _logger.LogWarning("Total polling time ({TotalTime} seconds) exceeded to maximum allowed ({MaxTime} seconds).",
                                    totalPollingTime.TotalSeconds,
                                    _maxWaitTime.TotalSeconds);                                
                                return default;
                            }
                            
                            break;
                        default:
                            _logger.LogError("An error occurred while polling for the response. The response status code was {StatusCode}.", responseMessage.StatusCode);
                            return default;
                    }
                    await Task.Delay(currentPollingInterval, cancellationToken);
                    // Exponential backoff
                    currentPollingInterval = TimeSpan.FromSeconds(currentPollingInterval.TotalSeconds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while polling for the response.");
                return default;
            }
        }
    }
}
