using FoundationaLLM.Common.Constants.Orchestration;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Response
{
    /// <summary>
    /// An OpenAI text message content item.
    /// </summary>
    public class TextMessageContentItem : MessageContentItemBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string? Type { get; set; }

        /// <summary>
        /// A list of file annotations used to generate the message content item.
        /// </summary>
        [JsonPropertyName("annotations")]
        public List<FilePathContentItem> Annotations { get; set; } = [];

        /// <summary>
        /// The text value of the message content item.
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }

        /// <summary>
        /// Set default property values.
        /// </summary>
        public TextMessageContentItem() =>
            Type = MessageContentItemTypes.Text;
    }
}
