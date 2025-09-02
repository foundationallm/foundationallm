using FoundationaLLM.Common.Models.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents the common properties of a knowledge entity used in a knowledge graph.
    /// </summary>
    public class KnowledgeEntityBase : KnowledgeItemBase
    {
        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        [JsonPropertyName("type")]
        [JsonPropertyOrder(-50)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonPropertyOrder(-49)]
        public string Name { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public override string UniqueId => $"{Type}:{Name}";

        /// <inheritdoc/>
        [JsonIgnore]
        public override string BucketId => $"Entity-{Type}";

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeEntity"/> class.
        /// </summary>
        public KnowledgeEntityBase()
        {
            Type = null!;
            Name = null!;
        }
    }
}
