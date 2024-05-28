namespace FoundationaLLM.Core.Examples.Models
{
    /// <summary>
    /// Generate conversations test configuration
    /// </summary>
    public class GenerateConversationsConfiguration
    {
        /// <summary>
        /// The optional path of the folder where the conversations should be persisted.
        /// </summary>
        public string? ResultFolderPath { get; set; }

        /// <summary>
        /// The number of synthetic conversations to generate.
        /// </summary>
        public int ConversationCount { get; set; } = 1;

        /// <summary>
        /// The number of threads to use for parllel execution.
        /// </summary>
        public int ThreadCount { get; set; } = 1;
    }
}
