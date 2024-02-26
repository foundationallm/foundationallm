using System.Text.Json.Serialization;
using FoundationaLLM.Common.Models.Configuration.Users;

namespace FoundationaLLM.Common.Models.Agents
{
    /// <summary>
    /// Represents the name and privacy of a FoundationaLLM agent.
    /// Used as a reference in the <see cref="UserProfile"/> class.
    /// </summary>
    public class AgentHint
    {
        /// <summary>
        /// The name of the agent.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// Indicates whether the agent is private.
        /// </summary>
        [JsonPropertyName("private")]
        public bool Private { get; set; }
    }
}
