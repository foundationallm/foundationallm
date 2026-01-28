using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentTemplates
{
    /// <summary>
    /// Represents a template used to create agents.
    /// </summary>
    public class AgentTemplate : ResourceBase
    {
        /// <summary>
        ///  Gets or sets the version of the agent template.
        /// </summary>
        [JsonPropertyName("version")]
        public required string Version { get; set; }
    }
}
