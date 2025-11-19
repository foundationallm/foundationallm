namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Represents the result of a content safety analysis.
    /// </summary>
    public class ContentSafetyAnalysisResult
    {
        /// <summary>
        /// Gets or sets a flag representing if the analysis was executed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets a flag representing if the content is safe or not.
        /// </summary>
        public bool SafeContent { get; set; }

        /// <summary>
        /// Gets or sets the reason why the content was considered to be unsafe.
        /// </summary>
        public string? Details { get; set; }
    }
}
