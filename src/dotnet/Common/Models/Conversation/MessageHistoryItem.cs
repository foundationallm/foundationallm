using FoundationaLLM.Common.Models.Orchestration.Response;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Conversation
{
    /// <summary>
    /// Represents an historic message sender and text item.
    /// </summary>
    public class MessageHistoryItem
    {
        /// <summary>
        /// The sender of the message (e.g. "Agent", "User").
        /// </summary>
        [JsonPropertyName("sender")]
        public string Sender { get; set; }
        /// <summary>
        /// The message text.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the list <see cref="ContentArtifact"/> objects.
        /// </summary>
        /// <remarks>
        /// Not all content artifacts are loaded.
        /// The content of the list depends on the content artifact types configured in the message history settings of the agent.
        /// </remarks>
        [JsonPropertyName("content_artifacts")]
        public List<ContentArtifact>? ContentArtifacts { get; set; }

        /// <summary>
        /// Message history item
        /// </summary>
        /// <param name="sender">The sender of the message (e.g., "Agent", "User")</param>
        /// <param name="text">The message text.</param>
        public MessageHistoryItem(string sender, string text)
        {
            Sender = sender;
            Text = text;
        }
    }
}
