using FoundationaLLM.Common.Constants;
using System.Diagnostics;

namespace FoundationaLLM.Common.Telemetry
{
    /// <summary>
    /// Provides predefined telemetry activity sources for the components of the platform.
    /// </summary>
    public class TelemetryActivitySources
    {
        /// <summary>
        /// The activity source for the Core API.
        /// </summary>
        public static readonly ActivitySource CoreAPIActivitySource = new (ServiceNames.CoreAPI);

        /// <summary>
        /// The activity source for the Orchestration API.
        /// </summary>
        public static readonly ActivitySource OrchestrationAPIActivitySource = new (ServiceNames.OrchestrationAPI);

        /// <summary>
        /// The activity source for the Data Pipeline Frontend Worker Service.
        /// </summary>
        public static readonly ActivitySource DataPipelineFrontendWorkerActivitySource = new (ServiceNames.DataPipelineFrontendWorker);

        /// <summary>
        /// The activity source for the Data Pipeline Backend Worker Service.
        /// </summary>
        public static readonly ActivitySource DataPipelineBackendWorkerActivitySource = new (ServiceNames.DataPipelineBackendWorker);

        /// <summary>
        /// A collection of all predefined activity sources.
        /// </summary>
        public static readonly IEnumerable<ActivitySource> All = new[]
        {
            CoreAPIActivitySource,
            OrchestrationAPIActivitySource,
            DataPipelineFrontendWorkerActivitySource,
            DataPipelineBackendWorkerActivitySource
        };
    }
}
