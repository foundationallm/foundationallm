using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Context.Knowledge
{
    /// <summary>
    /// Represents a request to list knowledge resources.
    /// </summary>
    public class ContextKnowledgeResourceListRequest
    {
        /// <summary>
        /// Gets or sets the optional list of knowledge resource names to filter the results.
        /// </summary>
        [JsonPropertyName("knowledge_resource_names")]
        public List<string>? KnowledgeResourceNames { get; set; }
    }
}
