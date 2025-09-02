using FoundationaLLM.Common.Models.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents the common properties of a knowledge relationship used in a knowledge graph.
    /// </summary>
    public class KnowledgeRelationshipBase : KnowledgeItemBase
    {
        /// <summary>
        /// Gets or sets the source entity of the relationship.
        /// </summary>
        [JsonPropertyName("source")]
        [JsonPropertyOrder(-50)]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the type of the source entity.
        /// </summary>
        [JsonPropertyName("source_type")]
        [JsonPropertyOrder(-49)]
        public string SourceType { get; set; }

        /// <summary>
        /// Gets or sets the target entity of the relationship.
        /// </summary>
        [JsonPropertyName("target")]
        [JsonPropertyOrder(-48)]
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the type of the target entity.
        /// </summary>
        [JsonPropertyName("target_type")]
        [JsonPropertyOrder(-47)]
        public string TargetType { get; set; }

        /// <summary>
        /// Gets the unique identifier for the source, combining the source type and source value.
        /// </summary>
        [JsonIgnore]
        public string SourceUniqueId => $"{SourceType}:{Source}";

        /// <summary>
        /// Gets the unique identifier for the target, combining the target type and target value.
        /// </summary>
        [JsonIgnore]
        public string TargetUniqueId => $"{TargetType}:{Target}";

        /// <inheritdoc/>
        [JsonIgnore]
        public override string UniqueId => $"{SourceUniqueId}->{TargetUniqueId}";

        /// <inheritdoc/>
        [JsonIgnore]
        public override string BucketId => $"Relationship-{SourceType}";

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeRelationship"/> class.
        /// </summary>
        public KnowledgeRelationshipBase()
        {
            Source = null!;
            SourceType = null!;
            Target = null!;
            TargetType = null!;
        }
    }
}
