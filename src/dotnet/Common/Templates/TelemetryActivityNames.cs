namespace FoundationaLLM.Common.Constants.Telemetry
{
    /// <summary>
    /// Defines constants for all telemetry activity names.
    /// </summary>
    public static class TelemetryActivityNames
    {
        /// <summary>
        /// The telemetry activity name for the CoreAPI AsyncCompletions_StartCompletionOperation action.
        /// </summary>
        public const string CoreAPI_AsyncCompletions_StartCompletionOperation = "AsyncCompletions_StartCompletionOperation";

        /// <summary>
        /// The telemetry activity name for the CoreAPI Completions_GetCompletion action.
        /// </summary>
        public const string CoreAPI_Completions_GetCompletion = "Completions_GetCompletion";

        /// <summary>
        /// The telemetry activity name for the CoreAPI Files_Upload action.
        /// </summary>
        public const string CoreAPI_Files_Upload = "Files_Upload";

        /// <summary>
        /// The telemetry activity name for the CoreAPI Files_Downlaod action.
        /// </summary>
        public const string CoreAPI_Files_Download = "Files_Download";

        /// <summary>
        /// The telemetry activity name for the user prompt rewrite LLM invocation action in the Orchestration API.
        /// </summary>
        public const string OrchestrationAPI_AgentOrchestration_UserPromptRewrite_LLM = "UserPromptRewrite_LLM";

    }
}
