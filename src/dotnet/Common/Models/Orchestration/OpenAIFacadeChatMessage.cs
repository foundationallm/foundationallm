using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Represents a message in a chat completion response.
    /// </summary>
    public class OpenAIFacadeChatMessage
    {
        /// <summary>
        /// The role of the message author (always "assistant" for responses).
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = "assistant";

        /// <summary>
        /// The content of the message.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
