using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.DataPipelineEngine.Services.Queueing
{
    /// <summary>
    /// Maintains a registry of payloads that are currently being processed.
    /// </summary>
    /// <param name="messageQueueService">The message queue service providing queueing capabilities.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class ProcessingPayloadsRegistry<T>(
        IMessageQueueService<T> messageQueueService,
        ILogger logger)
    {
        private readonly IMessageQueueService<T> _messageQueueService = messageQueueService;
        private readonly ILogger _logger = logger;

        private Dictionary<string, DequeuedMessage<T>> _messageRegistry = [];
        private readonly SemaphoreSlim _messageRegistrySemaphore = new(1, 1);

        /// <summary>
        /// Indicates whether there are new payloads available for processing.
        /// </summary>
        /// <returns><see langword="true"/> if there are payloads available, <see langword="false"/> otherwise.</returns>
        public async Task<bool> NewPayloadsAvailable() =>
            await _messageQueueService.MessagesAvailable()
                .ConfigureAwait(false);

        /// <summary>
        /// Extends the processing time for the specified payloads.
        /// </summary>
        /// <param name="payloadIds">The list of payload identifiers for which processing time is extended.</param>
        /// <param name="recoverFromError">Indicates whether the extension is required to attempt to recover from an error.</param>
        /// <returns></returns>
        /// <remarks>
        /// This operation ensures that the specified payloads can continue to be processed without
        /// running the risk of being dequeued due to timeout and generating a processing race condition.
        /// </remarks>
        public async Task ExtendPayloadsProcessingTime(
            IEnumerable<string> payloadIds,
            bool recoverFromError = false)
        {
            _logger.LogInformation("Extending processing time for payloads: {PayloadIds}", 
                string.Join(", ", payloadIds));

            foreach (var payloadId in payloadIds)
                if (_messageRegistry.TryGetValue(payloadId, out var message))
                {
                    var updatedMessage =
                        await _messageQueueService.UpdateMessageVisibilityTimeout(
                            message,
                            recoverFromError: recoverFromError)
                        .ConfigureAwait(false);

                    if (updatedMessage != null)
                        try
                        {
                            await _messageRegistrySemaphore.WaitAsync()
                                .ConfigureAwait(false);
                            _messageRegistry[payloadId] = updatedMessage;
                        }
                        finally
                        {
                            _messageRegistrySemaphore.Release();
                        }
                    else
                        _logger.LogError("Failed to extend processing time for payload {PayloadId}.", payloadId);
                }
                else
                    _logger.LogError("Payload {PayloadId} not found in registry.", payloadId);
        }

        /// <summary>
        /// Receives a specified number of payloads for processing.
        /// </summary>
        /// <param name="payloadsCount">The maximum number of payloads to receive.</param>
        /// <param name="idSelector">The identifier selector for a payload.</param>
        /// <returns>A list containing the received payloads.</returns>
        public async Task<IEnumerable<T>> ReceivePayloadsForProcessing(
            int payloadsCount,
            Func<T,string> idSelector)
        {
            var dequeuedMessages =
                await _messageQueueService.ReceiveMessages(payloadsCount)
                    .ConfigureAwait(false);

            var validatedDequeuedMessages = new List<DequeuedMessage<T>>();

            try
            {
                await _messageRegistrySemaphore.WaitAsync()
                    .ConfigureAwait(false);

                foreach (var dequeuedMessage in dequeuedMessages)
                {
                    var payloadId = idSelector(dequeuedMessage.Message);
                    if (_messageRegistry.ContainsKey(payloadId))
                        _logger.LogError(
                            "Payload with identifier {PayloadId} already exists in the processing payloads registry.",
                            payloadId);
                    else
                    {
                        _messageRegistry[payloadId] = dequeuedMessage;
                        validatedDequeuedMessages.Add(dequeuedMessage);
                    }
                }
            }
            finally
            {
                _messageRegistrySemaphore.Release();
            }

            return validatedDequeuedMessages
                .Select(vdm => vdm.Message)
                .ToList();
        }

        /// <summary>
        /// Removes a payload.
        /// </summary>
        /// <param name="payloadId">The identifier of the payload to remove.</param>
        /// <param name="deleteMessage">Indicates whether the underlying message should be deleted as well.</param>
        public async Task RemovePayload(
            string payloadId,
            bool deleteMessage = true)
        {
            if (_messageRegistry.TryGetValue(payloadId, out var dequeuedMessage))
            {
                if (deleteMessage)
                {
                    if (!await _messageQueueService.DeleteMessage(dequeuedMessage)
                        .ConfigureAwait(false))
                        _logger.LogError("Failed to delete message for payload {PayloadId}.", payloadId);
                    else
                        _logger.LogInformation("Deleted message for payload {PayloadId}.", payloadId);
                }

                try
                {
                    await _messageRegistrySemaphore.WaitAsync()
                        .ConfigureAwait(false);
                    _messageRegistry.Remove(payloadId);
                }
                finally
                {
                    _messageRegistrySemaphore.Release();
                }
            }
            else
                _logger.LogWarning("Payload {PayloadId} not found in registry.", payloadId);
        }

        /// <summary>
        /// Ignores a list of payloads.
        /// </summary>
        /// <param name="payloadIds">The list of payload identifiers to ignore.</param>
        public async Task IgnorePayloads(
            IEnumerable<string> payloadIds)
        {
            foreach (var payloadId in payloadIds)
                await RemovePayload(
                    payloadId,
                    deleteMessage: false);
        }
    }
}
