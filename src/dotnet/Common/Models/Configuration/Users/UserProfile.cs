using FoundationaLLM.Common.Constants;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Configuration.Users
{
    /// <summary>
    /// The user profile object persisted in long-term storage.
    /// </summary>
    /// <param name="UPN">The user's account user principal name.</param>
    public record UserProfile(string UPN)
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
        /// A dictionary of <see cref="UserProfileFlags"/>.
        /// </summary>
        [JsonPropertyName("flags")]
        public Dictionary<string, bool> Flags { get; set; } = UserProfileFlags.All.ToDictionary(key => key, value => false);

        /// <summary>
        /// Gets or sets the list of agents the user selected for their profile.
        /// </summary>
        /// <remarks>
        /// The list contains the resource object identifiers of the agents.
        /// </remarks>
        [JsonPropertyName("agents")]
        public List<string> Agents { get; set; } = [];

        /// <summary>
        /// Gets or sets the date and time when the user profile was last updated.
        /// </summary>
        [JsonPropertyName("updated_on")]
        public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.MinValue;
    }
}
