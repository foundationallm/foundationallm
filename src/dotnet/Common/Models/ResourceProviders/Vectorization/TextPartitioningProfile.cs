using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Provides details about a text partitioning profile.
    /// </summary>
    public class TextPartitioningProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the text splitter.
        /// </summary>
        [JsonPropertyName("text_splitter")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required TextSplitterType TextSplitter { get; set; }
    }
}
