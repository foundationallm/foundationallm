using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authentication
{
    /// <summary>
    /// Represents strongly-typed user identity information, regardless of
    /// the identity provider.
    /// </summary>
    public class UnifiedUserIdentity
    {
        /// <summary>
        /// The user's display name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// The username of the user used to authenticate.
        /// </summary>
        [JsonPropertyName("user_name")]
        public string? Username { get; set; }
        /// <summary>
        /// The User Principal Name (UPN) of the user.
        /// </summary>
        [JsonPropertyName("upn")]
        public string? UPN { get; set; }
        /// <summary>
        /// The membership group ids of the user.
        /// </summary>
        [JsonPropertyName("group_ids")]
        public List<string>? GroupIds { get; set; }
    }
}
