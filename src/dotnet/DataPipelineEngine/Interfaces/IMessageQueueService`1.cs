using FoundationaLLM.DataPipelineEngine.Models;

namespace FoundationaLLM.DataPipelineEngine.Interfaces
{
    /// <summary>
    /// Defines the interface for a message queue service.
    /// </summary>
    public interface IMessageQueueService<T>
    {
        /// <summary>
        /// Inidicates whether there are messages available in the queue.
        /// </summary>
        /// <returns><see langword="true"/> if there are messages available, <see langword="false"/> otherwise.</returns>
        Task<bool> MessagesAvailable();

        /// <summary>
        /// Receives a specified number of messages from the queue.
        /// </summary>
        /// <param name="messageCount">The number of messages to receive.</param>
        /// <returns>A list of dequeued messages.</returns>
        Task<IEnumerable<DequeuedMessage<T>>> ReceiveMessages(
            int messageCount);

        /// <summary>
        /// Updates the visibility timeout of a message in the queue.
        /// </summary>
        /// <param name="message">The message whose visibility timeout is updated.</param>
        /// <param name="recoverFromError">Indicates whether the reason for the update is the need to recover from an error or not.</param>
        /// <returns>A copy of the original dequeued message with the updated visibility timeout.</returns>
        Task<DequeuedMessage<T>?> UpdateMessageVisibilityTimeout(
            DequeuedMessage<T> message,
            bool recoverFromError = false);

        /// <summary>
        /// Deletes a message from the queue.
        /// </summary>
        /// <param name="message">The message to be deleted.</param>
        /// <returns><see langword="true"/> if the message was successfully deleted, <see langword="false"/> otherwise.</returns>
        Task<bool> DeleteMessage(DequeuedMessage<T> message);
    }
}
