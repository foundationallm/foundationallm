using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.AzureAIService
{
    /// <summary>
    /// The GPT connections for the Azure AI Service.
    /// </summary>
    public class GptConnections
    {
        /// <summary>
        /// The GPT Coherence connection
        /// </summary>
        [JsonPropertyName("gpt_coherence")]
        public GptConnection? GptCoherence { get; set; }
        /// <summary>
        /// The uri to the data set.
        /// </summary>
        [JsonPropertyName("gpt_fluency")]
        public GptConnection? GptFluency { get; set; }
        /// <summary>
        /// The uri to the data set.
        /// </summary>
        [JsonPropertyName("gpt_groundedness")]
        public GptConnection? GptGroundedness { get; set; }
        /// <summary>
        /// The uri to the data set.
        /// </summary>
        [JsonPropertyName("gpt_relevance")]
        public GptConnection? GptRelevance { get; set; }
        /// <summary>
        /// The uri to the data set.
        /// </summary>
        [JsonPropertyName("gpt_similarity")]
        public GptConnection? GptSimilarity { get; set; }

    }
}
