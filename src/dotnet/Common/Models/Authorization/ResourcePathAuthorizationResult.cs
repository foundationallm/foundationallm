namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents the result of a resource path authorization request.
    /// </summary>
    public class ResourcePathAuthorizationResult
    {
        /// <summary>
        /// Gets or sets the resource path that was authorized.
        /// </summary>
        public required string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource that was authorized.
        /// </summary>
        public string? ResourceName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resource path is authorized.
        /// </summary>
        public bool Authorized { get; set; }

        /// <summary>
        /// Gets or sets the list of roles that authorize the action for the resource path.
        /// </summary>
        /// <remarks>
        /// The list contains the display names of the roles (e.g., Reader, Contributor, Owner, etc.).
        /// </remarks>
        public List<string> Roles { get; set; } = [];

        /// <summary>
        /// Gets or sets the dictionary of <see cref="ResourcePathAuthorizationResult"/> objects representing
        /// authorization results for subordinate resource paths. They keys of the dictionary are the resource names.
        /// </summary>
        /// <remarks>
        /// This dictionary will only contain values if the resource path in <see cref="ResourcePath"/> is
        /// a resource type path and <see cref="ActionAuthorizationRequest.ExpandResourceTypePaths"/>
        /// was set to <c>true</c> on the request that generated this result.
        /// </remarks>
        public Dictionary<string, ResourcePathAuthorizationResult> SubordinateResourcePathsAuthorizationResults { get; set; } = [];
    }
}
