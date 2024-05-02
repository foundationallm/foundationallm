using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// Provides details about a text embedding profile.
    /// </summary>
    public class TextEmbeddingProfile : VectorizationProfileBase
    {
        /// <summary>
        /// The type of the text splitter.
        /// </summary>
        [JsonPropertyName("text_embedding")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required TextEmbeddingType TextEmbedding { get; set; }
    }
}
