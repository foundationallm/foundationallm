using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;


namespace FoundationaLLM.Common.Models.ResourceProviders.Prompt
{
    /// <summary>
    /// Multipart prompt composed of a prefix and a suffix.
    /// </summary>
    public class MultipartPrompt : PromptBase
    {
        /// <summary>
        /// The prompt prefix.
        /// </summary>
        [JsonPropertyName("prefix")]
        public string? Prefix { get; set; }
        /// <summary>
        /// The prompt suffix.
        /// </summary>
        [JsonPropertyName("suffix")]
        public string? Suffix { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public MultipartPrompt() =>
            Type = PromptTypes.Multipart;
    }

}
