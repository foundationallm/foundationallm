namespace FoundationaLLM.Common.Constants.DataPipelines
{
    /// <summary>
    /// Constants for well-known data pipeline trigger parameter names.
    /// </summary>
    public static class DataPipelineTriggerParameterNames
    {
        /// <summary>
        /// The context file object identifier for the ContextFile data source.
        /// </summary>
        public const string DataSourceContextFileContextFileObjectId = "DataSource.ContextFile.ContextFileObjectId";

        /// <summary>
        /// The content action for the ContextFile data source.
        /// </summary>
        public const string DataSourceContextFileContentAction = "DataSource.ContextFile.ContentAction";

        /// <summary>
        /// The maximum content size in characters for the extract stage.
        /// </summary>
        public const string StageExtractMaxContentSizeCharacters = "Stage.Extract.MaxContentSizeCharacters";

        /// <summary>
        /// The knowledge unit object identifier for the embed stage.
        /// </summary>
        public const string StageEmbedKnowledgeUnitObjectId = "Stage.Embed.KnowledgeUnitObjectId";

        /// <summary>
        /// The knowledge unit object identifier for the index stage.
        /// </summary>
        public const string StageIndexKnowledgeUnitObjectId = "Stage.Index.KnowledgeUnitObjectId";

        /// <summary>
        /// The vector store identifier for the indexing stage.
        /// </summary>
        public const string StageIndexVectorStoreId = "Stage.Index.VectorStoreId";

        /// <summary>
        /// The knowledge unit object identifier for the removal stage.
        /// </summary>
        public const string StageRemovalKnowledgeUnitObjectId = "Stage.Removal.KnowledgeUnitObjectId";

        /// <summary>
        /// The vector store identifier for the removal stage.
        /// </summary>
        public const string StageRemovalVectorStoreId = "Stage.Removal.VectorStoreId";
    }
}
