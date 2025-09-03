using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Configuration.Users
{
    /// <summary>
    /// Represents a request to update the properties of a user profile.
    /// </summary>
    public class UserProfileUpdateRequest
    {
        /// <summary>
        /// Gets or sets the object identifier of the agent to add or remove from the user's profile. 
        /// </summary>
        [JsonPropertyName("agent_object_id")]
        public string? AgentObjectId { get; set; }
    }
}
