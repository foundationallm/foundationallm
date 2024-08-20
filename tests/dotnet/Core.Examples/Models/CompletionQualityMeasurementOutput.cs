namespace FoundationaLLM.Core.Examples.Models
{
    /// <summary>
    /// Contains the output of the completion quality measurement job run.
    /// </summary>
    public class CompletionQualityMeasurementOutput
    {
        /// <summary>
        /// Azure AI evaluation Job ID.
        /// </summary>
        public Guid? JobID { get; set; }
        /// <summary>
        /// The agent prompt that was used.
        /// </summary>
        public string? UserPrompt { get; set; }
        /// <summary>
        /// The agent completion that was generated.
        /// </summary>
        public string? AgentCompletion { get; set; }
        /// <summary>
        /// The agent completion that was expected.
        /// </summary>
        public string? ExpectedCompletion { get; set; }
    }
}
