using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    public class VectorizationStep
    {
        [JsonPropertyOrder(0)]
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyOrder(1)]
        [JsonPropertyName("parameters")]
        public required Dictionary<string, string> Parameters { get; set; }
    }
}
