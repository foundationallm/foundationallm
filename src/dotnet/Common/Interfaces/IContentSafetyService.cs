using FoundationaLLM.Common.Models.ContentSafety;

namespace FoundationaLLM.Common.Interfaces;

/// <summary>
/// Interface for calling a content safety service.
/// </summary>
public interface IContentSafetyService
{
    /// <summary>
    /// Checks if a text is safe or not based on pre-configured content filters.
    /// </summary>
    /// <param name="content">The text content that needs to be analyzed.</param>
    /// <returns>The text analysis restult, which includes a boolean flag that represents if the content is considered safe. 
    /// In case the content is unsafe, also returns the reason.</returns>
    Task<ContentSafetyAnalysisResult> AnalyzeText(string content);

    /// <summary>
    /// Detects attempted prompt injections and jailbreaks in user prompts.
    /// </summary>
    /// <param name="content">The text content that needs to be analyzed.</param>
    /// <returns>The text analysis restult, which includes a flag indicating whether the content contains prompt injections or jailbreaks.</returns>
    Task<ContentSafetyAnalysisResult> DetectPromptInjection(string content);

    /// <summary>
    /// Analyzes a collection of documents to detect potential prompt injection attacks.
    /// </summary>
    /// <param name="context">The context in which the documents are being analyzed.</param>
    /// <param name="documents">An enumerable collection of documents to analyze for prompt injection.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the analysis results for the documents.</returns>
    Task<ContentSafetyDocumentAnalysisResult> DetectPromptInjection(
        string context,
        IEnumerable<ContentSafetyDocument> documents,
        CancellationToken cancellationToken);
}
