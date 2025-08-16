using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Context
{
    /// <summary>
    /// Represents a FoundationaLLM knowledge source.
    /// </summary>
    public class KnowledgeSource : ResourceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeSource"/> class and sets its type to <see
        /// cref="ContextTypes.KnowledgeSource"/>.
        /// </summary>
        public KnowledgeSource() =>
            Type = ContextTypes.KnowledgeSource;

        /// <summary>
        /// Gets or sets the list of the object identifiers of the knowledge units associated with the knowledge source.
        /// </summary>
        [JsonPropertyName("knowledge_unit_object_ids")]
        public List<string> KnowledgeUnitObjectIds { get; set; } = [];
    }
}
