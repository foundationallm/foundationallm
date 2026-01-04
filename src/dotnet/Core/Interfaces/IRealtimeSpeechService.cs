using System.Net.WebSockets;
using FoundationaLLM.Common.Models.RealtimeSpeech;

namespace FoundationaLLM.Core.Interfaces
{
    /// <summary>
    /// Service for handling realtime speech-to-speech communication.
    /// </summary>
    public interface IRealtimeSpeechService
    {
        /// <summary>
        /// Handles a WebSocket connection for realtime speech.
        /// </summary>
        Task HandleWebSocketConnection(
            string instanceId,
            string sessionId,
            string agentName,
            WebSocket webSocket,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets the realtime speech configuration for an agent.
        /// </summary>
        Task<RealtimeSpeechConfiguration> GetConfigurationAsync(
            string instanceId,
            string agentName);
    }
}
