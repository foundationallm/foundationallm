using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Orchestration.Response
{
    /// <summary>
    /// Represents a single choice in an OpenAI chat completion response.
    /// </summary>
    public class OpenAIFacadeChatChoice
    {
        /// <summary>
        /// The index of this choice in the list of choices.
        /// </summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }

        /// <summary>
        /// The generated message.
        /// </summary>
        [JsonPropertyName("message")]
        public OpenAIFacadeChatMessage Message { get; set; } = new();

        /// <summary>
        /// The reason why the completion stopped.
        /// </summary>
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }
}
