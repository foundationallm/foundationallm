using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.AzureAIService
{
    /// <summary>
    /// Batch data input class.
    /// </summary>
    public class BatchDataInput
    {
        /// <summary>
        /// The uri to the data set.
        /// </summary>
        [JsonPropertyName("dataUri")]
        public string DataUri { get; set; }
    }
}
