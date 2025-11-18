using FoundationaLLM.Common.Models.ContentSafety;
using System.ClientModel;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Represents a client for interacting with Azure AI Content Safety services.
    /// </summary>
    public interface IAzureAIContentSafetyClient
    {
        /// <summary>
        /// Analyzes the specified text and returns the results of the analysis.
        /// </summary>
        /// <param name="request">The request containing the text to analyze.</param>
        /// <param name="cancellationToken">The optional cancellation token used to signal a cancellation request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  <see
        /// cref="ClientResult"/> object with the results of the analysis.</returns>
        Task<ClientResult<AnalyzeTextResult>> AnalyzeText(
            AnalyzeTextRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes a user prompt and associated documents for various prompt attacks.
        /// </summary>
        /// <param name="request">The request containing the user prompt and associated documents.</param>
        /// <param name="cancellationToken">The optional cancellation token used to signal a cancellation request.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an  <see
        /// cref="ClientResult"/> object with the results of the analysis.</returns>
        Task<ClientResult<ShieldPromptResult>> ShieldPrompt(
            ShieldPromptRequest request,
            CancellationToken cancellationToken = default);
    }
}
