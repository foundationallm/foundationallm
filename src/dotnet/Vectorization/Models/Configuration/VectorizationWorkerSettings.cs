using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    public class VectorizationWorkerSettings
    {
        [JsonPropertyOrder(0)]
        public List<RequestManagerServiceSettings>? RequestManagers { get; set; }

        [JsonPropertyOrder(1)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationQueuing QueuingEngine { get; set; }
    }
}
