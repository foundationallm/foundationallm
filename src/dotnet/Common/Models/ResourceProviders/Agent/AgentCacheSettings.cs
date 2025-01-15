using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides agent-related caching settings.
    /// </summary>
    public class AgentCacheSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the agent's semantic cache is enabled.
        /// </summary>
        /// <remarks>
        /// When enabled, the agent's semantic cache settings are provided in <see cref="SemanticCacheSettings"/>.
        /// </remarks>
        [JsonPropertyName("semantic_cache_enabled")]
        public bool SemanticCacheEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the agent's semantic cache settings.
        /// </summary>
        /// <remarks>
        /// The values in this property are only valid when <see cref="SemanticCacheEnabled"/> is <see langword="true"/>.
        /// </remarks>
        [JsonPropertyName("semantic_cache_settings")]
        public AgentSemanticCacheSettings? SemanticCacheSettings { get; set; }
    }
}
