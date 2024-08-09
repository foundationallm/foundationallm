﻿namespace FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// Provides configuration options for the Gatekeeper service.
    /// </summary>
    public record GatekeeperServiceSettings
    {
        /// <summary>
        /// Flag for enabling or disabling the Azure Content Safety feature.
        /// </summary>
        public required bool EnableAzureContentSafety { get; set; }

        /// <summary>
        /// Flag for enabling or disabling the Azure Content Safety Prompt Shields.
        /// </summary>
        public required bool EnableAzureContentSafetyPromptShield { get; set; }

        /// <summary>
        /// Flag for enabling or disabling the Microsoft Presidio feature.
        /// </summary>
        public required bool EnableMicrosoftPresidio { get; set; }

        /// <summary>
        /// Flag for enabling or disabling the Lakera Guard feature.
        /// </summary>
        public required bool EnableLakeraGuard { get; set; }

        /// <summary>
        /// Flag for enabling or disabling the Enkrypt Guardrails feature.
        /// </summary>
        public required bool EnableEnkryptGuardrails { get; set; }
    }
}
