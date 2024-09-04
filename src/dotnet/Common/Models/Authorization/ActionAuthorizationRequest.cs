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
        /// Gets or sets a value indicating whether to expand resource type paths that are not authorized.
        /// </summary>
        /// <remarks>
        /// If the action specified by <see cref="Action"/> is not authorized for a resource type path, and this property is set to <c>true</c>, the response will include any authorized resource paths matching the resource type path.
        /// </remarks>
        public required bool ExpandResourceTypePaths { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include roles in the response.
        /// </summary>
        /// <remarks>
        /// If this property is set to <c>true</c>, for each authrorized resource path, the response will include the roles from the role assignments that authorize the action for the resource path.
        /// </remarks>
        public required bool IncludeRoles { get; set; }

        /// <summary>
        /// The <see cref="UserAuthorizationContext"/> containing the authorization context for the user.
        /// </summary>
        [JsonPropertyName("user_context")]
        public required UserAuthorizationContext UserContext { get; set; }
    }
}
