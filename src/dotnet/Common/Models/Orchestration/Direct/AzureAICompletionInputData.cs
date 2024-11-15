using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// Input data for a direct request to an Azure AI model.
    /// </summary>
    public class AzureAICompletionInputData
    {
        /// <summary>
        /// Object defining the required input role and content key value pairs.
        /// </summary>
        [JsonPropertyName("input_string")]
        public CompletionMessage[]? InputString { get; set; }

        /// <summary>
        /// Model configuration parameters.
        /// </summary>
        [JsonPropertyName("parameters")]
        public AzureAICompletionParameters? Parameters { get; set; }
    }
}
