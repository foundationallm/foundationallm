using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Defines the filter criteria for agents.
    /// </summary>
    public class AgentFilter
    {
        /// <summary>
        /// Gets or sets the name of the agent to filter by.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
