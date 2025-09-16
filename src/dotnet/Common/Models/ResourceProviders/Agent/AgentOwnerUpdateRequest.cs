using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Represents a request to update the owner of an agent.
    /// </summary>
    public class AgentOwnerUpdateRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier of the user who is the owner of the agent.
        /// </summary>
        /// <remarks>
        /// The user must have an Owner role assigned on the agent.
        /// </remarks>
        [JsonPropertyName("owner_user_id")]
        public required string OwnerUserId { get; set; }
    }
}
