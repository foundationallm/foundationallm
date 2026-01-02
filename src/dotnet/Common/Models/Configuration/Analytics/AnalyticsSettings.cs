namespace FoundationaLLM.Common.Models.Configuration.Analytics
{
    /// <summary>
    /// Settings for FoundationaLLM analytics features (binds to configuration section "FoundationaLLM:Analytics").
    /// </summary>
    public class AnalyticsSettings
    {
        /// <summary>
        /// Enables or disables analytics collection and processing.
        /// </summary>
        public bool Enabled { get; set; } = true;

        // Secrets may be Key Vault references in configuration

        /// <summary>
        /// Salt used to anonymize identifiers before storing or processing analytics data.
        /// May be a Key Vault reference (e.g. @Microsoft.KeyVault(...)).
        /// </summary>
        public string? AnonymizationSalt { get; set; }

        /// <summary>
        /// Workspace identifier for Azure Log Analytics where telemetry can be sent.
        /// </summary>
        public string? LogAnalyticsWorkspaceId { get; set; }

        /// <summary>
        /// Shared key for the Log Analytics workspace; may be a Key Vault reference.
        /// </summary>
        public string? LogAnalyticsSharedKey { get; set; }

        // Operational settings

        /// <summary>
        /// Duration, in minutes, that in-memory analytics cache entries are retained.
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 5;

        /// <summary>
        /// Number of days to retain analytics data before it is deleted or archived.
        /// </summary>
        public int RetentionDays { get; set; } = 90;

        /// <summary>
        /// When true, enables real-time analytics updates; otherwise analytics are processed in batches.
        /// </summary>
        public bool EnableRealTimeUpdates { get; set; } = false;

        /// <summary>
        /// Configuration for abuse-detection heuristics used by analytics.
        /// </summary>
        public AbuseDetectionSettings AbuseDetection { get; set; } = new();

        /// <summary>
        /// Settings that control abuse detection thresholds and windows.
        /// </summary>
        public class AbuseDetectionSettings
        {
            /// <summary>
            /// Enables or disables abuse detection features.
            /// </summary>
            public bool Enabled { get; set; } = true;

            /// <summary>
            /// Number of requests in the monitored interval considered a high request rate.
            /// </summary>
            public int HighRequestRateThreshold { get; set; } = 100;

            /// <summary>
            /// Number of requests in the monitored interval considered an extreme request rate.
            /// </summary>
            public int ExtremeRequestRateThreshold { get; set; } = 500;

            /// <summary>
            /// Count of rapid consecutive requests that indicate rapid-fire abuse when exceeded.
            /// </summary>
            public int RapidFireThreshold { get; set; } = 20;

            /// <summary>
            /// Time window in minutes used to evaluate rapid-fire behavior.
            /// </summary>
            public int RapidFireWindowMinutes { get; set; } = 1;

            /// <summary>
            /// Number of continuous usage hours considered suspicious for additional checks.
            /// </summary>
            public int ContinuousUsageHours { get; set; } = 20;

            /// <summary>
            /// Number of distinct agents accessed within the agent-hopping window considered abusive.
            /// </summary>
            public int AgentHoppingThreshold { get; set; } = 10;

            /// <summary>
            /// Time window in minutes used to evaluate agent-hopping behavior.
            /// </summary>
            public int AgentHoppingWindowMinutes { get; set; } = 60;

            /// <summary>
            /// Maximum number of file uploads within the monitored period considered abusive.
            /// </summary>
            public int FileUploadAbuseCount { get; set; } = 50;

            /// <summary>
            /// Total uploaded file size in GB within the monitored period considered abusive.
            /// </summary>
            public int FileUploadAbuseSizeGB { get; set; } = 1;

            /// <summary>
            /// Percentage (0-100) of error responses within the monitored interval considered a high error rate.
            /// </summary>
            public int HighErrorRateThreshold { get; set; } = 30;
        }
    }
}
