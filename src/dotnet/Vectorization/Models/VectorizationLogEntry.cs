using System.Text.Json.Serialization;

namespace FoundationaLLM.Vectorization.Models
{
    public class VectorizationLogEntry
    {
        [JsonPropertyOrder(0)]
        [JsonPropertyName("rid")]
        public string RequestId { get; set; }

        [JsonPropertyOrder(1)]
        [JsonPropertyName("t")]
        public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyOrder(2)]
        [JsonPropertyName("src")]
        public string Source { get; set; }

        [JsonPropertyOrder(3)]
        [JsonPropertyName("txt")]
        public string Text { get; set; }

        public VectorizationLogEntry(string requestId, string source, string text)
        {
            this.RequestId = requestId;
            this.Source = source;
            this.Text = text;
        }
    }
}
