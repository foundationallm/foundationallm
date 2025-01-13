using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides agent-related user prompt rewrite settings.
    /// </summary>
    public class AgentUserPromptRewriteSettings
    {
        /// <summary>
        /// Gets or sets the object identifier of the AI model to use for the user prompt rewriting.
        /// </summary>
        [JsonPropertyName("user_prompt_rewrite_ai_model_object_id")]
        public required string UserPromptRewriteAIModelObjectId { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the prompt to use for the user prompt rewriting.
        /// </summary>
        [JsonPropertyName("user_prompt_rewrite_prompt_object_id")]
        public required string UserPromptRewritePromptObjectId { get; set; }

        /// <summary>
        /// Gets or sets the window size for the user prompt rewriting.
        /// </summary>
        [JsonPropertyName("user_prompts_window_size")]
        public required int UserPromptsWindowSize { get; set; }
    }
}
