using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.AzureAIService
{
    /// <summary>
    /// A data set version request.
    /// </summary>
    public class DatasetVersionRequest
    {
        /// <summary>
        /// The container to save the data to.
        /// </summary>
        [JsonPropertyName("dataContainerName")]
        public string DataContainerName { get; set; }

        /// <summary>
        /// The data type of the data.
        /// </summary>
        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        /// <summary>
        /// The uri to the data set.
        /// </summary>
        [JsonPropertyName("dataUri")]
        public string DataUri { get; set; }

        /// <summary>
        /// Any mutable properties to pass to the data
        /// </summary>
        [JsonPropertyName("mutableProps")]
        public Dictionary<string, string>? MutableProps { get; set; }

        /// <summary>
        /// Using registered data.
        /// </summary>
        [JsonPropertyName("isRegistered")]
        public bool IsRegistered { get; set; }
    }
}
