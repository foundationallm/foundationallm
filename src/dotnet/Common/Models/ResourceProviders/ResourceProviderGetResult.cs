using FoundationaLLM.Common.Models.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    public class ResourceProviderGetResult
    {
        [JsonPropertyName("actions")]
        public required List<string> Actions { get; set; }

        [JsonPropertyName("roles")]
        public required List<string> Roles { get; set; }
    }

    public class AgentResourceProviderGetResult : ResourceProviderGetResult
    {
        [JsonPropertyName("agent")]
        public required AgentBase Agent { get; set; }
    }
}
