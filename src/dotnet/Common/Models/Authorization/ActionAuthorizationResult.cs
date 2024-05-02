using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents the result of an action authorization request.
    /// </summary>
    public class ActionAuthorizationResult
    {
        /// <summary>
        /// Indicates whether the action is authorized or not for each resource path.
        /// </summary>
        [JsonPropertyName("authorization_results")]
        public required Dictionary<string, bool> AuthorizationResults { get; set; }

        /// <summary>
        /// Contains a list of invalid resource paths, for which authorization could not be completed.
        /// </summary>
        [JsonPropertyName("invalid_resources")]
        public List<string>? InvalidResourcePaths { get; set; }
    }
}
