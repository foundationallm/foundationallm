using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// Assistant input message for a direct orchestration request.
    /// </summary>
    public class AssistantCompletionMessage : CompletionMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssistantCompletionMessage"/> class.
        /// </summary>
        public AssistantCompletionMessage() =>
            Role = InputMessageRoles.Assistant;

        /// <summary>
        /// An optional name for the participant. Provides the model information
        /// to differentiate between participants of the same role.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }
    }
}
