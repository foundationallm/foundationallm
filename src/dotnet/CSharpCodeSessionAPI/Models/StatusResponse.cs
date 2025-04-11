using System.Text.Json.Serialization;

namespace FoundationaLLM.CSharpCodeSession.API.Models
{
    /// <summary>
    /// Basic response model that indicates the status of an operation.
    /// </summary>
    public class StatusResponse
    {
        /// <summary>
        /// Gets or sets the status of the upload operation.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "success";
    }
}
