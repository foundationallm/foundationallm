using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Prompt
{
    /// <summary>
    /// Defines the filter criteria for prompts.
    /// </summary>
    public class PromptFilter
    {
        /// <summary>
        /// Gets or sets the name of the prompt to filter by.
        /// </summary>
        [JsonPropertyName("prompt_name")]
        public string? PromptName { get; set; }
    }
}
