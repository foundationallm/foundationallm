using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.AzureAI
{
    /// <summary>
    /// Provides an Azure AI project configuration for resources managed by the FoundationaLLM.AzureAI resource manager.
    /// </summary>
    public class AzureAIProject : ResourceBase
    {
        /// <summary>
        /// The Azure AI project connection string for resources managed by the FoundationaLLM.AzureAI resource manager.
        ///</summary>
        [JsonPropertyName("project_connection_string")]
        public required string ProjectConnectionString { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public AzureAIProject() =>
            Type = AzureAITypes.Project;

    }
}
