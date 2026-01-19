using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides user-specific overrides to the default code session endpoint provider configuration.
    /// </summary>
    /// <remarks>
    /// This override allows routing code execution sessions to a custom endpoint for a given user principal
    /// (UPN), instead of using the globally configured provider. It is primarily intended for local
    /// development, testing, or controlled troubleshooting scenarios where developers need to target
    /// non-production or experimental services without changing the shared configuration.
    ///
    /// Security considerations:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     Custom endpoints should only point to trusted, internal, and appropriately secured services.
    ///     Routing code execution to arbitrary or untrusted endpoints can expose sensitive data,
    ///     credentials, or execution capabilities.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     Ensure that any overridden endpoint enforces authentication/authorization and uses TLS.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     In production environments, use this feature sparingly and under governance controls (e.g.,
    ///     audited configuration changes) to avoid accidental or malicious redirection of code sessions.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    public class CodeSessionEndpointProviderOverride
    {
        /// <summary>
        /// Gets or sets the user principal name (UPN) associated with the override.
        /// </summary>
        [JsonPropertyName("upn")]
        public required string UPN { get; set; }

        /// <summary>
        /// Gets or sets the endpoint URL of the code session service.
        /// </summary>
        [JsonPropertyName("endpoint")]
        public required string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the override is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public required bool Enabled { get; set; }
    }
}
