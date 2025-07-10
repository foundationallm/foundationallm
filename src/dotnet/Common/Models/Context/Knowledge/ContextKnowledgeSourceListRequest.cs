using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to list knowledge sources.
    /// </summary>
    public class ContextKnowledgeSourceListRequest
    {
        /// <summary>
        /// Gets or sets the optional list of knowledge source names to filter the results.
        /// </summary>
        [JsonPropertyName("knowledge_source_names")]
        public List<string>? KnowledgeSourceNames { get; set; }
    }
}
