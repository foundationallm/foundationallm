namespace FoundationaLLM.Core.Models.Configuration
{
    /// <summary>
    /// Provides settings for the CoreService.
    /// </summary>
    public class CoreServiceSettings
    {
        /// <summary>
        /// The type of summarization for chat session names.
        /// </summary>
        public required ChatSessionNameSummarizationType SessionSummarization { get; set; }

        /// <summary>
        /// Controls whether the Gatekeeper API will be invoked or not.
        /// </summary>
        public required bool BypassGatekeeper {  get; set; }
    }
}
