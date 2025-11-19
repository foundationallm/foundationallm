namespace FoundationaLLM.Common.Models.ContentSafety
{
    /// <summary>
    /// Represents the result of a content safety analysis for multiple documents.
    /// </summary>
    public class ContentSafetyDocumentAnalysisResult
    {
        /// <summary>
        /// Gets or sets a flag representing if the analysis was executed successfully.
        /// </summary>
        /// <remarks>In this context, success means that there was at least one attempt to evaluate
        /// each document. The <see cref="DocumentResults"/> property might still contain documents
        /// for which the attempt was not successfully carried out.
        /// </remarks>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the analysis results for each processed document, indexed by document identifier.
        /// </summary>
        /// <remarks>Each entry in the dictionary maps a unique document identifier to its corresponding content
        /// safety analysis result. The dictionary will be empty if no documents have been analyzed.</remarks>
        public Dictionary<int, ContentSafetyAnalysisResult> DocumentResults { get; set; } = [];

        /// <summary>
        /// Gets or sets the details associated with the analysis, such as error messages if the analysis was not successful.
        /// </summary>
        public string? Details { get; set; }
    }
}
