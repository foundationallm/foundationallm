using Azure;
using FoundationaLLM.Common.Clients.Http;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Response;
using Microsoft.AspNetCore.Http;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Text.Json;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides methods for managing state for long-running operations.
    /// </summary>
    public class StateServiceClient : IStateServiceClient
    {
        private readonly Uri _endpoint;
        private readonly APIEndpointClientOptions _options;
        private readonly ClientPipeline _pipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateServiceClient"/> class.
        /// </summary>
        /// <param name="endpoint">The endpoint URI of the FoundationaLLM State API service.</param>
        /// <param name="credential">The <see cref="AzureKeyCredential"/> used to authenticate requests to the service.</param>
        /// <param name="options">Optional configuration settings for the client, such as retry policies and timeouts.</param>
        public StateServiceClient(
            Uri endpoint,
            AzureKeyCredential credential,
            APIEndpointClientOptions? options)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _options = options ?? new();

            var auth = ApiKeyAuthenticationPolicy.CreateHeaderApiKeyPolicy(
                credential,
                _options.APIKeyHeaderName ?? "api-key",
                _options.APIKeyPrefix);

            _pipeline = ClientPipeline.Create(
                _options,
                perCallPolicies: [],
                perTryPolicies: [auth],
                beforeTransportPolicies: []);
        }

        /// <summary>
        /// Creates and configures a new instance of the <see cref="StateServiceClient"/>.
        /// </summary>
        /// <param name="clientBuilderParameters">A dictionary of parameters used to further configure the client.</param>
        /// <returns>A new instance of the <see cref="StateServiceClient"/> configured with the specified
        /// <paramref name="clientBuilderParameters"/>.</returns>
        public static StateServiceClient BuildClient(
            Dictionary<string, object> clientBuilderParameters) =>
            (AuthenticationTypes)clientBuilderParameters[HttpClientFactoryServiceKeyNames.AuthenticationType] switch
            {
                AuthenticationTypes.APIKey => new StateServiceClient(
                    new Uri(clientBuilderParameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString()!),
                    new AzureKeyCredential(clientBuilderParameters[HttpClientFactoryServiceKeyNames.APIKey].ToString()!),
                    APIEndpointClientOptions.FromClientBuilderParameters(clientBuilderParameters)),
                _ => throw new ConfigurationValueException(
                    $"The {clientBuilderParameters[HttpClientFactoryServiceKeyNames.AuthenticationType]} authentication type is not supported by the FoundationaLLM State API client.")
            };

        /// <inheritdoc/>
        public async Task<ClientResult<LongRunningOperation>> CreateOperation(
            string instanceId,
            string operationId,
            UnifiedUserIdentity userIdentity,
            CancellationToken cancellationToken = default)
        {
            using var message = _pipeline.CreateMessage();
            var pipelineRequest = message.Request;

            pipelineRequest.Method = HttpMethods.Post;
            pipelineRequest.Uri = GetRequestUri($"/instances/{instanceId}/operations/{operationId}");
            pipelineRequest.Headers.Add("Accept", "application/json");
            pipelineRequest.Headers.Add("Content-Type", "application/json");

            var payload = BinaryData.FromObjectAsJson(new
            {
                operation_id = operationId,
                instance_id = instanceId,
                upn = userIdentity.UPN
            });
            pipelineRequest.Content = BinaryContent.Create(payload);

            message.Apply(new RequestOptions
            {
                CancellationToken = cancellationToken
            });

            await _pipeline.SendAsync(message).ConfigureAwait(false);

            if (message.Response!.IsError)
                throw new ClientResultException(message.Response);

            return ClientResult<LongRunningOperation>.FromValue<LongRunningOperation>(
                JsonSerializer.Deserialize<LongRunningOperation>(
                    message.Response.Content)!,
                message.Response);
        }

        /// <inheritdoc/>
        public async Task<ClientResult<LongRunningOperation>> UpdateOperation(
            string instanceId,
            string operationId,
            OperationStatus status,
            string statusMessage,
            UnifiedUserIdentity userIdentity,
            CancellationToken cancellationToken = default)
        {
            using var message = _pipeline.CreateMessage();
            var pipelineRequest = message.Request;

            pipelineRequest.Method = HttpMethods.Put;
            pipelineRequest.Uri = GetRequestUri($"/instances/{instanceId}/operations/{operationId}");
            pipelineRequest.Headers.Add("Accept", "application/json");
            pipelineRequest.Headers.Add("Content-Type", "application/json");
            var payload = BinaryData.FromObjectAsJson(new LongRunningOperation
            {
                OperationId = operationId,
                Status = status,
                StatusMessage = statusMessage,
                UPN = userIdentity.UPN
            });
            pipelineRequest.Content = BinaryContent.Create(payload);

            message.Apply(new RequestOptions
            {
                CancellationToken = cancellationToken
            });

            await _pipeline.SendAsync(message).ConfigureAwait(false);

            if (message.Response!.IsError)
                throw new ClientResultException(message.Response);

            return ClientResult<LongRunningOperation>.FromValue<LongRunningOperation>(
                JsonSerializer.Deserialize<LongRunningOperation>(
                    message.Response.Content)!,
                message.Response);
        }

        /// <inheritdoc/>
        public async Task UpdateOperationResult(
            string instanceId,
            string operationId,
            CompletionResponse completionResponse,
            CancellationToken cancellationToken = default)
        {
            using var message = _pipeline.CreateMessage();
            var pipelineRequest = message.Request;

            pipelineRequest.Method = HttpMethods.Post;
            pipelineRequest.Uri = GetRequestUri($"/instances/{instanceId}/operations/{operationId}/result");
            pipelineRequest.Headers.Add("Accept", "application/json");
            pipelineRequest.Headers.Add("Content-Type", "application/json");

            var payload = BinaryData.FromObjectAsJson(completionResponse);
            pipelineRequest.Content = BinaryContent.Create(payload);

            message.Apply(new RequestOptions
            {
                CancellationToken = cancellationToken
            });

            await _pipeline.SendAsync(message).ConfigureAwait(false);

            if (message.Response!.IsError)
                throw new ClientResultException(message.Response);
        }

        /// <inheritdoc/>
        public async Task<ClientResult<LongRunningOperation>> UpdateOperationWithTextResult(
            string instanceId,
            string operationId,
            OperationStatus status,
            string statusMessage,
            string resultMessage,
            UnifiedUserIdentity userIdentity,
            CancellationToken cancellationToken = default)
        {
            await UpdateOperationResult(
                instanceId,
                operationId,
                new CompletionResponse(
                    operationId,
                    resultMessage,
                    string.Empty, 0, 0, null),
                cancellationToken);

            var operation = await UpdateOperation(
                instanceId,
                operationId,
                status,
                statusMessage,
                userIdentity,
                cancellationToken);

            return operation;
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
