namespace FoundationaLLM.Core.Models.Configuration
{
    /// <summary>
    /// Types of summarization for chat session names.
    /// </summary>
    public enum ChatSessionNameSummarizationType
    {
        /// <summary>
        /// Name summarization containing the UTC time of the creation of the chat session.
        /// </summary>
        Timestamp = 0,
        /// <summary>
        /// Name summarization produced by a LLM via a completion requesting summarization.
        /// </summary>
        LLM
    }
}
