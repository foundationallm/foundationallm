using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Analytics
{
    /// <summary>
    /// Summary of analytics for a model.
    /// </summary>
    public class ModelAnalyticsSummary
    {
        /// <summary>
        /// The name of the model.
        /// </summary>
        [JsonPropertyName("model_name")]
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// The total number of times this model has been used.
        /// </summary>
        [JsonPropertyName("usage_count")]
        public int UsageCount { get; set; }

        /// <summary>
        /// The average response time in milliseconds.
        /// </summary>
        [JsonPropertyName("avg_response_time_ms")]
        public double AvgResponseTimeMs { get; set; }

        /// <summary>
        /// The average tokens consumed per completion.
        /// </summary>
        [JsonPropertyName("avg_tokens_per_completion")]
        public double AvgTokensPerCompletion { get; set; }

        /// <summary>
        /// The cost per request in USD.
        /// </summary>
        [JsonPropertyName("cost_per_request")]
        public double CostPerRequest { get; set; }

        /// <summary>
        /// The error rate as a percentage.
        /// </summary>
        [JsonPropertyName("error_rate")]
        public double ErrorRate { get; set; }
    }
}
