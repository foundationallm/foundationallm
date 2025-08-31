namespace FoundationaLLM.Common.Models.ResourceProviders.Authorization
{
    /// <summary>
    /// Represents a security principal, such as a user, group, or service, that can be assigned permissions or access
    /// rights.
    /// </summary>
    public class SecurityPrincipal : ResourceBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the security principal.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the email address associated with the security principal.
        /// </summary>
        public string? Email { get; set; }
    }
}
