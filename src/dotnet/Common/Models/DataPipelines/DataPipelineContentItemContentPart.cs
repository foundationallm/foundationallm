using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents a content part of a content item.
    /// </summary>
    public class DataPipelineContentItemContentPart : DataPipelineContentItemPartBase
    {
        /// <summary>
        /// Gets or sets the text content of the content item part.
        /// </summary>
        [JsonPropertyName("content")]
        [JsonPropertyOrder(1)]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the size of the content item part in tokens.
        /// </summary>
        [JsonPropertyName("content_size_tokens")]
        [JsonPropertyOrder(2)]
        public int ContentSizeTokens { get; set; }

        /// <summary>
        /// Gets or sets the embedding of the content item part.
        /// </summary>
        [JsonPropertyName("embedding")]
        [JsonPropertyOrder(3)]
        public float[]? Embedding { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="DataPipelineContentItemContentPart"/> with the specified parameters.
        /// </summary>
        /// <param name="contentItemCanonicalId">The canonical identifier of the content item.</param>
        /// <param name="position">The position of the part within the content item.</param>
        /// <param name="content">The content of the content item part.</param>
        /// <param name="contentSizeTokens">The size of the content item part in tokens.</param>
        /// <returns></returns>
        public static DataPipelineContentItemContentPart Create(
            string contentItemCanonicalId,
            int position,
            string content,
            int contentSizeTokens) =>
            new()
            {
                ContentItemCanonicalId = contentItemCanonicalId,
                Position = position,
                Content = content,
                ContentSizeTokens = contentSizeTokens,
                IndexEntryId = Convert.ToBase64String(
                    MD5.HashData(Encoding.UTF8.GetBytes(
                        $"{contentItemCanonicalId}-{position:D6}-{Guid.NewGuid()}")))
                    .Replace("+", "--")
                    .Replace("/", "--")
            };
    }
}
