using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    public class VectorizationWorkerSettings
    {
        [JsonPropertyOrder(0)]
        public List<RequestManagerServiceSettings>? RequestManagers { get; set; }

        [JsonPropertyOrder(1)]
        public VectorizationQueuing QueuingEngine { get; set; }
    }
}
