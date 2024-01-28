using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    /// <summary>
    /// Settings for the vectorization worker.
    /// </summary>
    public class VectorizationWorkerSettings
    {
        /// <summary>
        /// Settings for the request managers.
        /// </summary>
        [JsonPropertyOrder(0)]
        public List<RequestManagerServiceSettings>? RequestManagers { get; set; }

        /// <summary>
        /// Settings for the request sources.
        /// </summary>
        [JsonPropertyOrder(1)]
        public List<RequestSourceServiceSettings>? RequestSources { get; set; }

        /// <summary>
        /// The type of queuing engine used to dispatch vectorization requests.
        /// </summary>
        [JsonPropertyOrder(2)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationQueuing QueuingEngine { get; set; }
    }
}
