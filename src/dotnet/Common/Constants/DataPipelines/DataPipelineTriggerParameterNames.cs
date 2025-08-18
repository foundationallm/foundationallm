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
    }
}
