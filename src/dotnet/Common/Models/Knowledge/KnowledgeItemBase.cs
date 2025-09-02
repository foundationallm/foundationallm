using FoundationaLLM.Common.Models.DataPipelines;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Knowledge
{
    /// <summary>
    /// Represents the common properties of a knowledge item used in a data pipeline.
    /// </summary>
    public class KnowledgeItemBase : DataPipelineItemBase
    {
        /// <summary>
        /// Gets the unique identifier of the knowledge item.
        /// </summary>
        [JsonIgnore]
        public virtual string UniqueId => string.Empty;

        /// <summary>
        /// Gets the identifier of the bucket to which the knowledge item belongs.
        /// </summary>
        [JsonIgnore]
        public virtual string BucketId => string.Empty;
    }
}
