using System.Collections.Immutable;

namespace FoundationaLLM.Common.Constants.DataPipelines
{
    /// <summary>
    /// Constants for data pipeline run processors.
    /// </summary>
    public static class DataPipelineRunProcessors
    {
        /// <summary>
        /// The Data Pipeline Frontend Worker service.
        /// </summary>
        public const string Frontend = "DataPipelineFrontendWorker";

        /// <summary>
        /// The Data Pipeline Backend Worker service.
        /// </summary>
        public const string Backend = "DataPipelineBackendWorker";

        /// <summary>
        /// Contains all the data pipeline run processors.
        /// </summary>
        public readonly static ImmutableList<string> All = [
            Frontend,
            Backend
        ];
    }
}
