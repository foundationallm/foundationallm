using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Prompt
{
    /// <summary>
    /// String text token replacement definition.
    /// </summary>
    public class TokenReplacementDefinition
    {
        /// <summary>
        /// The token to be replaced.
        /// </summary>
        [JsonPropertyName("token")]
        public required string Token { get; set; }

        /// <summary>
        /// The code to compute the replacement value.
        /// </summary>
        [JsonPropertyName("compute_code")]
        public required string ComputeCode { get; set; }
    }
}
