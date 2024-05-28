namespace FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Provides configuration options for the Enkrypt Guardrails service.
    /// </summary>
    public record EnkryptGuardrailsServiceSettings
    {
        /// <summary>
        /// The Enkrypt Guardrails service endpoint.
        /// </summary>
        public required string APIUrl { get; init; }

        /// <summary>
        /// The Enkrypt Guardrails service key.
        /// </summary>
        public required string APIKey { get; init; }
    }
}
