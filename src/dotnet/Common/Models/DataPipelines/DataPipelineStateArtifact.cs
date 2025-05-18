namespace FoundationaLLM.Common.Models.DataPipelines
{
    /// <summary>
    /// Represents the state of a data pipeline artifact.
    /// </summary>
    public class DataPipelineStateArtifact
    {
        /// <summary>
        /// Gets or sets the file name of the artifact.
        /// </summary>
        public required string FileName { get; set; }

        /// <summary>
        /// Gets or sets the content type of the artifact.
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the binary content of the artifact.
        /// </summary>
        public required BinaryData Content { get; set; }
    }
}
