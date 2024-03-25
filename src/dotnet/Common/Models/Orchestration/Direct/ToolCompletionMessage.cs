using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// Tool input message for a direct orchestration request.
    /// </summary>
    public class ToolCompletionMessage : CompletionMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolCompletionMessage"/> class.
        /// </summary>
        public ToolCompletionMessage() =>
            Role = InputMessageRoles.Tool;

        /// <summary>
        /// Tool call to which this message is responding.
        /// </summary>
        [JsonPropertyName("tool_call_id")]
        public required string ToolCallId { get; set; }
    }
}
