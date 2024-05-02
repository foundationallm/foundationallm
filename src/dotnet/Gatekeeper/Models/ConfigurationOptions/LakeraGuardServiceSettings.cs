namespace FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Provides configuration options for the Lakera Guard service.
    /// </summary>
    public record LakeraGuardServiceSettings
    {
        /// <summary>
        /// The Lakera Guard service endpoint.
        /// </summary>
        public required string APIUrl { get; init; }

        /// <summary>
        /// The Lakera Guard service key.
        /// </summary>
        public required string APIKey { get; init; }
    }
}
