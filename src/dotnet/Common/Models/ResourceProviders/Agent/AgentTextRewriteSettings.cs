using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides agent-related text rewrite settings.
    /// </summary>
    public class AgentTextRewriteSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether user prompt rewrite is enabled for the agent.
        /// </summary>
        /// <remarks>
        /// When enabled, the agent's semantic cache settings are provided in <see cref="UserPromptRewriteSettings"/>.
        /// </remarks>
        [JsonPropertyName("user_prompt_rewrite_enabled")]
        public bool UserPromptRewriteEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the agent's semantic cache settings.
        /// </summary>
        /// <remarks>
        /// The values in this property are only valid when <see cref="UserPromptRewriteEnabled"/> is <see langword="true"/>.
        /// </remarks>
        [JsonPropertyName("user_prompt_rewrite_settings")]
        public AgentUserPromptRewriteSettings? UserPromptRewriteSettings { get; set; }
    }
}
