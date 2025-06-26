using FoundationaLLM.Common.Models.Knowledge;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents a knowledge part of a data pipeline content item.
    /// </summary>
    public class DataPipelineContentItemKnowledgePart : DataPipelineContentItemPartBase
    {
        /// <summary>
        /// Gets or sets the entities and relationships extracted from the content item part.
        /// </summary>
        [JsonPropertyName("entities")]
        public KnowledgeEntityRelationship<ExtractedKnowledgeEntity, ExtractedKnowledgeRelationship>? EntitiesAndRelationships { get; set; }

        /// <summary>
        /// Converts a content item part to a knowledge part.
        /// </summary>
        /// <param name="contentItemPart">The content item part to be converted.</param>
        /// <returns>A new content item knowledge part.</returns>
        public static DataPipelineContentItemKnowledgePart FromContentItemPart(
            DataPipelineContentItemPartBase contentItemPart) =>
            new()
            {
                ContentItemCanonicalId = contentItemPart.ContentItemCanonicalId,
                Position = contentItemPart.Position,
                IndexEntryId = contentItemPart.IndexEntryId,
                Metadata = contentItemPart.Metadata
            };
    }
}
