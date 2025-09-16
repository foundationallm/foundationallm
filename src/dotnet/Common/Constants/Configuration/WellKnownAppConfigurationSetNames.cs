namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Provides a collection of well-known names for application configuration sets.
    /// </summary>
    public static class WellKnownAppConfigurationSetNames
    {
        /// <summary>
        /// The application configuration set used by the user portal.
        /// </summary>
        public const string UserPortal = "UserPortal";

        /// <summary>
        /// The application configuration set used by the management portal.
        /// </summary>
        public const string ManagementPortal = "ManagementPortal";

        /// <summary>
        /// All application configuration sets.
        /// </summary>
        public readonly static string[] All = [
            UserPortal,
            ManagementPortal
        ];
    }
}
