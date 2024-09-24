using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents the result of an action authorization request.
    /// </summary>
    public class ActionAuthorizationResult
    {
        /// <summary>
        /// Gets or sets the dictionary containing <see cref="ResourcePathAuthorizationResult"/> objects representing the authorization result for each resource path.
        /// </summary>
        [JsonPropertyName("authorization_results")]
        public required Dictionary<string, ResourcePathAuthorizationResult> AuthorizationResults { get; set; }

        /// <summary>
        /// Gets or sets a list of invalid resource paths, for which authorization could not be completed.
        /// </summary>
        [JsonPropertyName("invalid_resources")]
        public List<string>? InvalidResourcePaths { get; set; }
    }
}
