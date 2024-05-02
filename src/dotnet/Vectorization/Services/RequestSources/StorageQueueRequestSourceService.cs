using Azure.Storage.Queues;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.Services.RequestSources
{
    /// <summary>
    /// Implements a request source that uses Azure storage queues.
    /// </summary>
    public class StorageQueueRequestSourceService : IRequestSourceService
    {
        private readonly RequestSourceServiceSettings _settings;
        private readonly ILogger<StorageQueueRequestSourceService> _logger;
        private readonly QueueClient _queueClient;

        /// <inheritdoc/>
        public string SourceName => _settings.Name;

        /// <summary>
        /// Creates a new instance of the request source.
        /// </summary>
        /// <param name="settings">The <see cref="RequestSourceServiceSettings"/> object providing the settings.</param>
        /// <param name="logger">The logger used for logging.</param>
        
        public StorageQueueRequestSourceService(
            RequestSourceServiceSettings settings,
            ILogger<StorageQueueRequestSourceService> logger
            )
        {
            _settings = settings;
            _logger = logger;

            var queueServiceClient = new QueueServiceClient(new Uri($"https://{_settings.AccountName}.queue.core.windows.net"), DefaultAuthentication.GetAzureCredential());
            _queueClient = queueServiceClient.GetQueueClient(_settings.Name);
        }

        /// <inheritdoc/>
        public async Task<bool> HasRequests()
        {
            try
            {
                var message = await _queueClient.PeekMessageAsync().ConfigureAwait(false);
                return message.Value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while attempting to peek at messages in queue {QueueName}.", _settings.Name);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<(VectorizationRequest Request, string MessageId, string PopReceipt, long DequeueCount)>> ReceiveRequests(int count)
        {
            var receivedMessages = await _queueClient.ReceiveMessagesAsync(count, TimeSpan.FromSeconds(_settings.VisibilityTimeoutSeconds)).ConfigureAwait(false);

            var result = new List<(VectorizationRequest, string, string, long)>();

            if (receivedMessages.HasValue)
            {
                foreach (var m in receivedMessages.Value)
                {
                    try
                    {
                        var vectorizationRequest = JsonSerializer.Deserialize<VectorizationRequest>(m.Body.ToString());
                                               
                        result.Add(new(
                            vectorizationRequest!,
                            m.MessageId,
                            m.PopReceipt,
                            m.DequeueCount));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Cannot deserialize message with id {MessageId}.", m.MessageId);
                    }
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task DeleteRequest(string messageId, string popReceipt) =>
            await _queueClient.DeleteMessageAsync(messageId, popReceipt).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task SubmitRequest(VectorizationRequest request)
        {
            var serializedMessage = JsonSerializer.Serialize(request);
            await _queueClient.SendMessageAsync(serializedMessage).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task UpdateRequest(string messageId, string popReceipt, VectorizationRequest request)
        {
            var serializedMessage = JsonSerializer.Serialize(request);
            await _queueClient.UpdateMessageAsync(messageId, popReceipt, serializedMessage);
        }
    }
}
