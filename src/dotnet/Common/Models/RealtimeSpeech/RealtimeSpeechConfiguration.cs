using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.RealtimeSpeech
{
    /// <summary>
    /// Configuration returned to the client for realtime speech capabilities.
    /// </summary>
    public class RealtimeSpeechConfiguration
    {
        /// <summary>
        /// Whether realtime speech is enabled for the agent.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Stop words that will terminate the session.
        /// </summary>
        [JsonPropertyName("stop_words")]
        public List<string> StopWords { get; set; } = new();

        /// <summary>
        /// The WebSocket endpoint URL for establishing a realtime connection.
        /// </summary>
        [JsonPropertyName("websocket_url")]
        public string? WebSocketUrl { get; set; }

        /// <summary>
        /// Voice identifier being used.
        /// </summary>
        [JsonPropertyName("voice")]
        public string? Voice { get; set; }
    }
}
