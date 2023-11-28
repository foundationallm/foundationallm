namespace FoundationaLLM.Gatekeeper.Core.Interfaces
{
    /// <summary>
    /// Contains methods for interacting with the Gatekeeper Integration API.
    /// </summary>
    public interface IGatekeeperIntegrationAPIService
    {
        /// <summary>
        /// Analyze text to identify PII (personally identifiable information) entities.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>A list of PII (personally identifiable information) entities identified in the analyzed text.</returns>
        Task<List<string>> AnalyzeText(string text);

        /// <summary>
        /// Anonymize text with identified PII (personally identifiable information) entities.
        /// </summary>
        /// <param name="text">The input text.</param>
        /// <returns>The anonymized text.</returns>
        Task<string> AnonymizeText(string text);
    }
}
