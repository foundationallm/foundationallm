using FoundationaLLM.Common.Constants.Agents;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// User input message for a direct orchestration request.
    /// </summary>
    public class UserCompletionMessage : CompletionMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCompletionMessage"/> class.
        /// </summary>
        public UserCompletionMessage() =>
            Role = InputMessageRoles.User;

        /// <summary>
        /// An optional name for the participant. Provides the model information
        /// to differentiate between participants of the same role.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }
    }
}
