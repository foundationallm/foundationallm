namespace FoundationaLLM.Common.Constants.DataPipelines
{
    /// <summary>
    /// Constants for well-known data pipeline state file names.
    /// </summary>
    public static class DataPipelineStateFileNames
    {
        /// <summary>
        /// The name of the file that contains metadata about a content item.
        /// </summary>
        public const string Metadata = "metadata.json";

        /// <summary>
        /// The name of the file that contains the text content of a content item.
        /// </summary>
        public const string TextContent = "content.txt";

        /// <summary>
        /// The name of the file that contains the partitioned content parts of a content item.
        /// </summary>
        public const string ContentParts = "content-parts.parquet";

        /// <summary>
        /// The name of the file that contains the partitioned knowledge parts of a content item.
        /// </summary>
        public const string KnowledgeParts = "knowledge-parts.parquet";
    }
}
