using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Provides the properties for AI models used for realtime speech-to-speech.
    /// </summary>
    public class RealtimeSpeechAIModel : AIModelBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="RealtimeSpeechAIModel"/> AI model.
        /// </summary>
        public RealtimeSpeechAIModel() =>
            Type = AIModelTypes.RealtimeSpeech;

        /// <summary>
        /// The voice to use for the model's audio responses.
        /// Options: alloy, echo, shimmer, etc.
        /// </summary>
        [JsonPropertyName("voice")]
        public string Voice { get; set; } = "alloy";

        /// <summary>
        /// The audio format for input. Default: pcm16
        /// </summary>
        [JsonPropertyName("input_audio_format")]
        public string InputAudioFormat { get; set; } = "pcm16";

        /// <summary>
        /// The audio format for output. Default: pcm16
        /// </summary>
        [JsonPropertyName("output_audio_format")]
        public string OutputAudioFormat { get; set; } = "pcm16";

        /// <summary>
        /// Turn detection configuration.
        /// </summary>
        [JsonPropertyName("turn_detection")]
        public TurnDetectionConfig? TurnDetection { get; set; }

        /// <summary>
        /// Whether input audio transcription is enabled.
        /// </summary>
        [JsonPropertyName("input_audio_transcription_enabled")]
        public bool InputAudioTranscriptionEnabled { get; set; } = true;

        /// <summary>
        /// The model to use for input audio transcription.
        /// </summary>
        [JsonPropertyName("input_audio_transcription_model")]
        public string? InputAudioTranscriptionModel { get; set; } = "whisper-1";

        /// <summary>
        /// The API key for authentication with the realtime speech service.
        /// </summary>
        [JsonPropertyName("api_key")]
        public string? ApiKey { get; set; }

        /// <summary>
        /// The endpoint URL for the realtime speech service.
        /// </summary>
        [JsonPropertyName("endpoint")]
        public string? Endpoint { get; set; }
    }

    /// <summary>
    /// Configuration for voice activity detection / turn detection.
    /// </summary>
    public class TurnDetectionConfig
    {
        /// <summary>
        /// Type of turn detection: server_vad or none
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "server_vad";

        /// <summary>
        /// Activation threshold (0.0 to 1.0)
        /// </summary>
        [JsonPropertyName("threshold")]
        public double Threshold { get; set; } = 0.5;

        /// <summary>
        /// Audio to include before speech starts (ms)
        /// </summary>
        [JsonPropertyName("prefix_padding_ms")]
        public int PrefixPaddingMs { get; set; } = 300;

        /// <summary>
        /// Duration of silence to detect speech stop (ms)
        /// </summary>
        [JsonPropertyName("silence_duration_ms")]
        public int SilenceDurationMs { get; set; } = 500;
    }
}
