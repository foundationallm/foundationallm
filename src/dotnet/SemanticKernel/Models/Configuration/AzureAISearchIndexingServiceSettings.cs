using FoundationaLLM.Common.Constants.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.SemanticKernel.Core.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings for the Azure AI Search indexing service.
    /// </summary>
    public record AzureAISearchIndexingServiceSettings
    {
        /// <summary>
        /// The endpoint of the Azure AI deployment.
        /// </summary>
        public required string Endpoint { get; set; }

        /// <summary>
        /// The <see cref="AuthenticationType"/> indicating which authentication mechanism to use.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required AuthenticationTypes AuthenticationType { get; set; }
    }
}
