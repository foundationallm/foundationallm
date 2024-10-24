using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Provides information about the OpenAI files associated with an agent.
    /// </summary>
    public class AgentFileUserContext
    {
        /// <summary>
        /// The Azure OpenAI endpoint used to manage the assistant.
        /// </summary>
        [JsonPropertyName("endpoint")]
        [JsonPropertyOrder(101)]
        public required string Endpoint { get; set; }

        /// <summary>
        /// The dictionary of <see cref="FileMapping"/> objects providing information about the files uploaded to the OpenAI assistant. 
        /// </summary>
        /// <remarks>
        /// The keys of the dictionary are the FoundationaLLM attachment identifiers.
        /// </remarks>
        [JsonPropertyName("files")]
        [JsonPropertyOrder(102)]
        public Dictionary<string, FileMapping> Files { get; set; } = [];
    }
}
