using System.Text.Json.Serialization;

namespace FoundationaLLM.Gatekeeper.Core.Models.EnkryptGuardrails
{
    /// <summary>
    /// Recognizes patterns that may indicate an prompt injection attack or jailbreak attempt protecting the system from malicious inputs.
    /// </summary>
    public class DetectResult
    {
        /// <summary>
        /// Detection summary.
        /// </summary>
        [JsonPropertyName("summary")]
        public required DetectSummary Summary { get; set; }

        /// <summary>
        /// Detection details.
        /// </summary>
        [JsonPropertyName("details")]
        public required DetectDetails Details { get; set; }
    }

    /// <summary>
    /// Detection summary that contains flags for each detector.
    /// </summary>
    public class DetectSummary
    {
        /// <summary>
        /// The detector returns a binary value (0 for benign, 1 for attack).
        /// </summary>
        [JsonPropertyName("injection_attack")]
        public int? InjectionAttack { get; set; }
    }

    /// <summary>
    /// Detection details for each flag.
    /// </summary>
    public class DetectDetails
    {
        /// <summary>
        /// Prompt injection or jailbreak confidence levels.
        /// </summary>
        [JsonPropertyName("injection_attack")]
        public InjectionAttack? InjectionAttack { get; set; }
    }

    /// <summary>
    /// Prompt injection or jailbreak confidence levels.
    /// </summary>
    public class InjectionAttack
    {
        /// <summary>
        /// Confidence level for safe.
        /// </summary>
        [JsonPropertyName("safe")]
        public double Safe { get; set; }

        /// <summary>
        /// Confidence level for attack.
        /// </summary>
        [JsonPropertyName("attack")]
        public double Attack { get; set; }
    }
}
