using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using FoundationaLLM.Common.Clients.Http;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ContentSafety;
using Microsoft.AspNetCore.Http;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides methods for interacting with Azure AI Content Safety services.
    /// </summary>
    public class AzureAIContentSafetyClient : IAzureAIContentSafetyClient
    {
        private readonly Uri _endpoint;
        private readonly APIEndpointClientOptions _options;
        private readonly ClientPipeline _pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAIContentSafetyClient"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint URI of the Azure AI Content Safety service.</param>
        /// <param name="credential">The <see cref="AzureKeyCredential"/> used to authenticate requests to the service.</param>
        /// <param name="options">Optional configuration settings for the client, such as retry policies and timeouts.</param>
        /// <param name="enableRetry">Indicates whether to enable retry logic for transient failures.</param>
        public AzureAIContentSafetyClient(
            Uri endpoint,
            AzureKeyCredential credential,
            APIEndpointClientOptions? options,
            bool enableRetry)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _options = options ?? new();

            var auth = ApiKeyAuthenticationPolicy.CreateHeaderApiKeyPolicy(
                credential,
                _options.APIKeyHeaderName ?? "api-key",
                _options.APIKeyPrefix);

            if (enableRetry)
                _options.RetryPolicy = new ClientRetryPolicy(
                    maxRetries: 5);

            _pipeline = ClientPipeline.Create(
                _options,
                perCallPolicies: [],
                perTryPolicies: [auth],
                beforeTransportPolicies: []);
        }

        /// <summary>
        /// Creates and configures a new instance of the <see cref="AzureAIContentSafetyClient"/>.
        /// </summary>
        /// <param name="clientBuilderParameters">A dictionary of parameters used to further configure the client.</param>
        /// <returns>A new instance of the <see cref="AzureAIContentSafetyClient"/> configured with the specified
        /// <paramref name="clientBuilderParameters"/>.</returns>
        public static IAzureAIContentSafetyClient BuildClient(
            Dictionary<string, object> clientBuilderParameters) =>
            (AuthenticationTypes)clientBuilderParameters[HttpClientFactoryServiceKeyNames.AuthenticationType] switch
            {
                AuthenticationTypes.APIKey => new AzureAIContentSafetyClient(
                    new Uri(clientBuilderParameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString()!),
                    new AzureKeyCredential(clientBuilderParameters[HttpClientFactoryServiceKeyNames.APIKey].ToString()!),
                    APIEndpointClientOptions.FromClientBuilderParameters(clientBuilderParameters),
                    clientBuilderParameters.ContainsKey(HttpClientFactoryServiceKeyNames.EnableRetry)
                        && (bool)clientBuilderParameters[HttpClientFactoryServiceKeyNames.EnableRetry]),
                _ => throw new ConfigurationValueException(
                    $"The {clientBuilderParameters[HttpClientFactoryServiceKeyNames.AuthenticationType]} authentication type is not supported by the Azure AI Content Safety client.")
            };

        /// <inheritdoc/>
        public async Task<ClientResult<AnalyzeTextResult>> AnalyzeText(
            AnalyzeTextRequest request,
            CancellationToken cancellationToken = default)
        {
            using var message = _pipeline.CreateMessage();
            var pipelineRequest = message.Request;

            pipelineRequest.Method = HttpMethods.Post;
            pipelineRequest.Uri = GetRequestUri("/contentsafety/text:analyze");
            pipelineRequest.Headers.Add("Accept", "application/json");
            pipelineRequest.Headers.Add("Content-Type", "application/json");

            var payload = BinaryData.FromObjectAsJson(request);
            pipelineRequest.Content = BinaryContent.Create(payload);

            message.Apply(new RequestOptions
            {
                CancellationToken = cancellationToken
            });

            await _pipeline.SendAsync(message).ConfigureAwait(false);

            if (message.Response!.IsError)
                throw new ClientResultException(message.Response);

            return ClientResult<AnalyzeTextResult>.FromValue<AnalyzeTextResult>(
                JsonSerializer.Deserialize<AnalyzeTextResult>(
                    message.Response.Content)!,
                message.Response);
        }

        /// <inheritdoc/>
        public async Task<ClientResult<ShieldPromptResult>> ShieldPrompt(
            ShieldPromptRequest request,
            CancellationToken cancellationToken = default)
        {
            using var message = _pipeline.CreateMessage();
            var pipelineRequest = message.Request;

            pipelineRequest.Method = HttpMethods.Post;
            pipelineRequest.Uri = GetRequestUri("/contentsafety/text:shieldPrompt");
            pipelineRequest.Headers.Add("Accept", "application/json");
            pipelineRequest.Headers.Add("Content-Type", "application/json");

            var payload = BinaryData.FromObjectAsJson(request);
            pipelineRequest.Content = BinaryContent.Create(payload);

            message.Apply(new RequestOptions
            {
                CancellationToken = cancellationToken
            });

            await _pipeline.SendAsync(message).ConfigureAwait(false);

            if (message.Response!.IsError)
                throw new ClientResultException(message.Response);

            return ClientResult<ShieldPromptResult>.FromValue<ShieldPromptResult>(
                JsonSerializer.Deserialize<ShieldPromptResult>(
                    message.Response.Content)!,
                message.Response);
        }

        private Uri GetRequestUri(string requestPath)
        {
            var baseUri = new Uri(_endpoint, requestPath);
            var uriBuilder = new UriBuilder(baseUri);

            if (!string.IsNullOrWhiteSpace(_options.APIVersion))
            {
                var query = uriBuilder.Query;
                if (!string.IsNullOrEmpty(query))
                    query += "&";
                query += $"api-version={_options.APIVersion}";
                uriBuilder.Query = query;
            }

            return uriBuilder.Uri;
        }
    }
}
