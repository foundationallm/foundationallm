using FoundationaLLM.Common.Models.ResourceProviders.Configuration;

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

        /// <summary>
        /// The comma-separated list file extensions that are supported by the Azure OpenAI Assistants file search tool.
        /// </summary>
        public required string AzureOpenAIAssistantsFileSearchFileExtensions { get; set; }

        /// <summary>
        /// The comma-separated list file extensions that are supported by the Azure AI Agent Service file search tool.
        /// </summary>
        public required string AzureAIAgentsFileSearchFileExtensions { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of file uploads that can be included in a single message.
        /// </summary>
        public ConfigurationValue<int> MaxUploadsPerMessage { get; set; } = new ConfigurationValue<int>() { Value = 5 };

        /// <summary>
        /// Gets or sets the polling interval, in milliseconds, used when waiting for completion responses from the
        /// service.
        /// </summary>
        public ConfigurationValue<int> CompletionResponsePollingIntervalMilliseconds { get; set; } = new ConfigurationValue<int>() { Value = 1000 };
    }
}
