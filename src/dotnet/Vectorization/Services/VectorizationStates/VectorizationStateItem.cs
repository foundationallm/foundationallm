using Parquet.Serialization.Attributes;

namespace FoundationaLLM.Vectorization.Services.VectorizationStates
{
    /// <summary>
    /// Properties that are serialized in the Parquet file associated with the vectorization state.
    /// Combines properties from the TextPartition and the TextEmbeddingVector artifacts.
    /// </summary>
    public class VectorizationStateItem
    {
        /// <summary>
        /// The position of the item in the list of vectorization artifacts.
        /// </summary>
        [ParquetRequired]
        public int Position { get; set; }

        /// <summary>
        /// The content of the TextPartition artifact.
        /// </summary>
        [ParquetRequired]
        public string? TextPartitionContent { get; set; }

        /// <summary>
        /// The MD5 hash of the TextPartition artifact content.
        /// </summary>
        [ParquetRequired]
        public string? TextPartitionHash { get; set; }

        /// <summary>
        /// The size of the TextPartition artifact content.
        /// </summary>
        [ParquetRequired]
        public int TextPartitionSize { get; set; }

        /// <summary>
        /// The size of the TextEmbeddingVector artifact content (number of embedding dimensions).
        /// </summary>
        public int TextEmbeddingVectorSize { get; set; }

        /// <summary>
        /// The content of the TextEmbeddingVector artifact.
        /// </summary>
        public List<float>? TextEmbeddingVector { get; set; }

        /// <summary>
        /// The MD5 of the TextEmbeddingVector artifact content in string format.
        /// </summary>
        public string? TextEmbeddingVectorHash { get; set; }
    }
}
