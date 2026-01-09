using System.Net.WebSockets;
using FoundationaLLM.Common.Models.RealtimeSpeech;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;

namespace FoundationaLLM.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for realtime speech providers.
    /// This abstraction allows supporting multiple speech-to-speech backends.
    /// </summary>
    public interface IRealtimeSpeechProvider
    {
        /// <summary>
        /// Gets the provider type identifier (e.g., "azure-openai-realtime", "azure-speech-sdk").
        /// </summary>
        string ProviderType { get; }

        /// <summary>
        /// Establishes a connection to the realtime speech backend.
        /// </summary>
        /// <param name="model">The AI model configuration.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A WebSocket connection to the backend.</returns>
        Task<WebSocket> ConnectAsync(RealtimeSpeechAIModel model, CancellationToken cancellationToken);

        /// <summary>
        /// Configures the session with the given instructions and settings.
        /// </summary>
        /// <param name="backendSocket">The backend WebSocket connection.</param>
        /// <param name="model">The AI model configuration.</param>
        /// <param name="instructions">System instructions for the session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task ConfigureSessionAsync(
            WebSocket backendSocket,
            RealtimeSpeechAIModel model,
            string? instructions,
            CancellationToken cancellationToken);

        /// <summary>
        /// Parses a message from the backend and extracts transcription if available.
        /// </summary>
        /// <param name="message">The raw message bytes.</param>
        /// <returns>Parsed result containing transcription data if present.</returns>
        RealtimeSpeechMessageResult ParseBackendMessage(byte[] message);

        /// <summary>
        /// Transforms a message from the client to the format expected by the backend.
        /// </summary>
        /// <param name="clientMessage">The client message bytes.</param>
        /// <returns>Transformed message for the backend.</returns>
        byte[] TransformClientMessage(byte[] clientMessage);
    }

    /// <summary>
    /// Result of parsing a backend message.
    /// </summary>
    public class RealtimeSpeechMessageResult
    {
        /// <summary>
        /// The message type.
        /// </summary>
        public string? MessageType { get; set; }

        /// <summary>
        /// User transcription, if this message contains one.
        /// </summary>
        public string? UserTranscription { get; set; }

        /// <summary>
        /// Agent transcription, if this message contains one.
        /// </summary>
        public string? AgentTranscription { get; set; }

        /// <summary>
        /// Indicates if this is an error message.
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Error message, if applicable.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
