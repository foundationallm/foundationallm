using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Defines a vectorization pipeline.
    /// </summary>
    public class VectorizationPipeline : ResourceBase
    {
        /// <summary>
        /// Indicates whether the pipeline is active or not.
        /// When the pipeline is inactive, it cannot be triggered to execute.
        /// </summary>
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        /// <summary>
        /// The object identifier of the data source used to retrieve content for vectorization.
        /// </summary>
        [JsonPropertyName("data_source_object_id")]
        public required string DataSourceObjectId { get; set; }

        /// <summary>
        /// The object identifier of the text partitioning profile used to split text.
        /// </summary>
        [JsonPropertyName("text_partitioning_profile_object_id")]
        public required string TextPartitioningProfileObjectId { get; set; }

        /// <summary>
        /// The object identifier of the text embedding profile used to embed text.
        /// </summary>
        [JsonPropertyName("text_embedding_profile_object_id")]
        public required string TextEmbeddingProfileObjectId { get; set; }

        /// <summary>
        /// The object identifier of the indexing profile used to index text embeddings.
        /// </summary>
        [JsonPropertyName("indexing_profile_object_id")]
        public required string IndexingProfileObjectId { get; set; }

        /// <summary>
        /// The type of trigger that initiates the execution of the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required VectorizationPipelineTriggerType TriggerType { get; set; }

        /// <summary>
        /// The schedule of the trigger in Cron format.
        /// This propoerty is valid only when TriggerType = Schedule.
        /// </summary>
        [JsonPropertyName("trigger_cron_schedule")]
        public string? TriggerCronSchedule { get; set; }


        /// <summary>
        /// The cost center associated with the pipeline.
        /// </summary>
        [JsonPropertyName("cost_center")]
        public string? CostCenter { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public VectorizationPipeline() =>
            Type = "vectorization-pipeline";
    }
}
