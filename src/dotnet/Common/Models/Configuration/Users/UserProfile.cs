using FoundationaLLM.Common.Models.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Configuration.Users
{
    /// <summary>
    /// The user profile object persisted in long-term storage.
    /// </summary>
    /// <param name="UPN">The user's account user principal name.</param>
    /// <param name="PrivateAgents">Private agents assigned to the user.</param>
    public record UserProfile(string UPN, IEnumerable<Agent>? PrivateAgents)
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = UPN;
        /// <summary>
        /// The document type.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; } = nameof(UserProfile);
        /// <summary>
        /// The user principal name.
        /// </summary>
        [JsonProperty("upn")]
        public string UPN { get; set; } = UPN;
        /// <summary>
        /// Names of private agents assigned to the user.
        /// </summary>
        [JsonProperty("privateAgents")]
        public IEnumerable<Agent>? PrivateAgents { get; set; } = PrivateAgents;
    }
}
