using FoundationaLLM.Common.Constants;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Configuration.Users
{
    /// <summary>
    /// The user data object persisted in long-term storage.
    /// </summary>
    /// <param name="UPN">The user's account user principal name.</param>
    public record UserData(string UPN)
    {
        /// <summary>
        /// The unique identifier.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = $"{UPN.ToLower()}|data";

        /// <summary>
        /// The document type.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = nameof(UserData);

        /// <summary>
        /// The user principal name.
        /// </summary>
        [JsonPropertyName("upn")]
        public string UPN { get; set; } = UPN;

        /// <summary>
        /// Gets or sets the list of agents the user has permission to use.
        /// </summary>
        /// <remarks>
        /// The list contains the resource object identifiers of the agents.
        /// </remarks>
        [JsonPropertyName("allowed_agents")]
        public List<string> AllowedAgents { get; set; } = [];

        /// <summary>
        /// Gets or sets the date and time when the user profile was last updated.
        /// </summary>
        [JsonPropertyName("updated_on")]
        public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.MinValue;

        /// <summary>
        /// Generates a unique identifier string based on the specified user principal name (UPN).
        /// </summary>
        /// <param name="upn">The user principal name to use as the basis for the identifier.</param>
        /// <returns>The requested identifier.</returns>
        public static string GetId(string upn) => $"{upn.ToLower()}|data";
    }
}
