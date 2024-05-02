using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.AzureAIService
{
    /// <summary>
    /// A connection to an Azure AI Service model.
    /// </summary>
    public class GptConnection
    {
        /// <summary>
        /// Name of the Azure AI studio connection.
        /// </summary>
        [JsonPropertyName("connection")]
        public string? Connection { get; set; }
        /// <summary>
        /// Name of the deployment to use.
        /// </summary>
        [JsonPropertyName("deployment_name")]
        public string? DeploymentName { get; set; }
    }
}
