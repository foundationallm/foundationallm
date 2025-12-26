using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.DataPipelineEngine.Services.Queueing
{
    /// <summary>
    /// Provides an implementation of the <see cref="IMessageQueueService"/> interface for Azure Storage Queues.
    /// </summary>
    public class AzureStorageQueueService<T> : IMessageQueueService<T>
    {
        private readonly string _queueName;
        private readonly QueueClient _queueClient;
        private readonly ILogger<AzureStorageQueueService<T>> _logger;

        private const int MESSAGE_VISIBILITY_TIMEOUT_SECONDS = 600;
        private const int MESSAGE_VISIBILITY_TIMEOUT_REFRESH_SECONDS = 300;
        private const int MESSAGE_ERROR_VISIBILITY_TIMEOUT_SECONDS = 5;

        /// <summary>
        /// Constructs a new instance of the <see cref="AzureStorageQueueService"/> class.
        /// </summary>
        /// <param name="storageAccountName">The name of the Azure Storage account.</param>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="logger">The logger used for logging.</param>
        public AzureStorageQueueService(
            string storageAccountName,
            string queueName,
            ILogger<AzureStorageQueueService<T>> logger)
        {
            _queueName = queueName;

            var queueServiceClient = new QueueServiceClient(
                new Uri($"https://{storageAccountName}.queue.core.windows.net"),
                ServiceContext.AzureCredential);
            _queueClient = queueServiceClient.GetQueueClient(queueName);

            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> MessagesAvailable()
        {
            try
            {
                var peekedMessageResponse = await _queueClient.PeekMessageAsync().ConfigureAwait(false);

                ValidateResponse<PeekedMessage>(peekedMessageResponse);
                return peekedMessageResponse.Value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while attempting to peek at messages in queue {QueueName}.",
                    _queueName);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DequeuedMessage<T>>> ReceiveMessages(
            int messageCount)
        {
            try
            {
                var receivedMessagesResponse = await _queueClient.ReceiveMessagesAsync(
                    messageCount,
                    TimeSpan.FromSeconds(MESSAGE_VISIBILITY_TIMEOUT_SECONDS)).ConfigureAwait(false);

                ValidateResponse<QueueMessage[]>(receivedMessagesResponse);
                var result = new List<DequeuedMessage<T>>();

                foreach (var m in receivedMessagesResponse.Value)
                {
                    try
                    {
                        result.Add(new DequeuedMessage<T>()
                        {
                            Message = JsonSerializer.Deserialize<T>(m.Body)!,
                            MessageId = m.MessageId,
                            PopReceipt = m.PopReceipt!,
                            DequeueCount = m.DequeueCount
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Cannot deserialize message with id {MessageId}.", m.MessageId);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while receiving messages from queue {QueueName}.", _queueName);
                return [];
            }
        }

        /// <inheritdoc/>
        public async Task<DequeuedMessage<T>?> UpdateMessageVisibilityTimeout(
            DequeuedMessage<T> message,
            bool recoverFromError = false)
        {
            try
            {
                // If recovering from an error we must update the visibility timeout
                if (!recoverFromError
                    && (DateTimeOffset.UtcNow - message.LastVisibilityTimeoutUpdate).TotalSeconds <=
                        MESSAGE_VISIBILITY_TIMEOUT_REFRESH_SECONDS)
                {
                    // No need to update visibility timeout yet
                    // (we want to avoid unnecessary updates)
                    return message;
                }

                var updateReceiptResponse = await _queueClient.UpdateMessageAsync(
                    message.MessageId,
                    message.PopReceipt,
                    visibilityTimeout: recoverFromError
                        ? TimeSpan.FromSeconds(MESSAGE_ERROR_VISIBILITY_TIMEOUT_SECONDS)
                        : TimeSpan.FromSeconds(MESSAGE_VISIBILITY_TIMEOUT_SECONDS)
                    ).ConfigureAwait(false);

                ValidateResponse<UpdateReceipt>(updateReceiptResponse);
                return new DequeuedMessage<T>
                {
                    Message = message.Message,
                    MessageId = message.MessageId,
                    PopReceipt = updateReceiptResponse.Value.PopReceipt!,
                    DequeueCount = message.DequeueCount,
                    LastVisibilityTimeoutUpdate = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the timeout visibility for message {MessageId} in queue {QueueName}.",
                    message.MessageId, _queueName);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMessage(DequeuedMessage<T> message)
        {
            try
            {
                var response = await _queueClient.DeleteMessageAsync(
                    message.MessageId,
                    message.PopReceipt).ConfigureAwait(false);

                ValidateResponse(response);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting message {MessageId} in queue {QueueName}.",
                    message.MessageId, _queueName);
                return false;
            }
        }

        private void ValidateResponse(
            Azure.Response response)
        {
            if (response.IsError)
                throw new RequestFailedException(response);
        }

        private void ValidateResponse<TResponse>(
            Azure.Response<TResponse> response)
        {
            var rawResponse = response.GetRawResponse();
            ValidateResponse(rawResponse);

            if (!response.HasValue)
            {
                throw new RequestFailedException(rawResponse);
            }
        }
    }
}
