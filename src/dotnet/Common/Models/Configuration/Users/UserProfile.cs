using FoundationaLLM.Common.Models.Agents;
using System.Text.Json.Serialization;


namespace FoundationaLLM.Common.Models.Configuration.Users
{
    /// <summary>
    /// The user profile object persisted in long-term storage.
    /// </summary>
    /// <param name="UPN">The user's account user principal name.</param>
    /// <param name="PrivateAgents">Private agents assigned to the user.</param>
    public record UserProfile(string UPN, IEnumerable<AgentHint>? PrivateAgents)
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = UPN;
        /// <summary>
        /// The document type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = nameof(UserProfile);
        /// <summary>
        /// The user principal name.
        /// </summary>
        [JsonPropertyName("upn")]
        public string UPN { get; set; } = UPN;
        /// <summary>
        /// Names of private agents assigned to the user.
        /// </summary>
        [JsonPropertyName("privateAgents")]
        public IEnumerable<AgentHint>? PrivateAgents { get; set; } = PrivateAgents;
    }
}
