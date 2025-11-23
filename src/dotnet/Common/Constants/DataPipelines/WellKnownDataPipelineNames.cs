namespace FoundationaLLM.Common.Constants.DataPipelines
{
    /// <summary>
    /// Provides a collection of well-known names for data pipelines.
    /// </summary>
    public static class WellKnownDataPipelineNames
    {
        /// <summary>
        /// The data pipeline used to process files uploaded to agent conversations (has no content safety step).
        /// </summary>
        public const string DefaultFileUpload = "DefaultFileUpload";

        /// <summary>
        /// The data pipeline used to process files uploaded to agent conversations (with content safety step).
        /// </summary>
        public const string ShieldedFileUpload = "ShieldedFileUpload";

        /// <summary>
        /// The data pipeline used to shield file content with content safety.
        /// </summary>
        public const string ShieldedFileContent = "ShieldedFileContent";

        /// <summary>
        /// All application configuration sets.
        /// </summary>
        public readonly static string[] All = [
            DefaultFileUpload,
            ShieldedFileUpload,
            ShieldedFileContent
        ];
    }
}
