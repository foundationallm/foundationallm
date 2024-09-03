using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a request to authorize an action.
    /// </summary>
    public class ActionAuthorizationRequest
    {
        /// <summary>
        /// The authorizable action for which authorization is requested.
        /// </summary>
        [JsonPropertyName("action")]
        public required string Action { get; set; }

        /// <summary>
        /// The list of resources for which authorization is requested.
        /// </summary>
        [JsonPropertyName("resources")]
        public required List<string> ResourcePaths { get; set; }

        /// <summary>
        /// The <see cref="UserAuthorizationContext"/> containing the authorization context for the user.
        /// </summary>
        [JsonPropertyName("user_context")]
        public required UserAuthorizationContext UserContext { get; set; }
    }
}
