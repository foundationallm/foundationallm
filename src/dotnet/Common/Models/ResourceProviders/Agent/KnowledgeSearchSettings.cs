namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Provides the settings for knowledge search.
    /// </summary>
    public class KnowledgeSearchSettings
    {
        /// <summary>
        /// The object identifier of the data pipeline used to process uploaded files.
        /// </summary>
        public required string FileUploadDataPipelineObjectId { get; set; }

        /// <summary>
        /// The object identifier of the knowledge unit used for the conversation files.
        /// </summary>
        public required string ConversationKnowledgeUnitObjectId { get; set; }

        /// <summary>
        /// The object identifier of the knowledge unit used for the agent private store files.
        /// </summary>
        public required string AgentPrivateStoreKnowledgeUnitObjectId { get; set; }

        /// <summary>
        /// Gets or sets the maximum context size in characters for content items processed by data pipelines.
        /// </summary>
        public int MaxContentSizeCharacters { get; set; } = 10000000;
    }
}
