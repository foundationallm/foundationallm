using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides agent-related gatekeeper settings.
    /// </summary>
    public class AgentGatekeeperSettings
    {
        /// <summary>
        /// Indicates whether to abide by or override the system settings for the Gatekeeper.
        /// </summary>
        [JsonPropertyName("use_system_setting")]
        public bool UseSystemSetting { get; set; }

        /// <summary>
        /// If <see cref="UseSystemSetting"/> is false, provides Gatekeeper feature selection.
        /// </summary>
        [JsonPropertyName("options")]
        public string[]? Options { get; set; }
    }
}
