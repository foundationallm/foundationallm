using FoundationaLLM.Common.Security;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.System
{
    internal class FoundationaLLMSystem : PermissionedObject
    {
        public FoundationaLLMSystem() { }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "version")]
        public Version Version { get; set; }
    }
}
