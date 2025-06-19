namespace FoundationaLLM.Plugins.DataPipeline
{
    public static class PluginParameterNames
    {
        public const string AZUREDATALAKE_DATASOURCE_FOLDERS = "Folders";

        public const string CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID = "ContextFileObjectId";

        public const string SHAREPOINTONLINE_DATASOURCE_DOCUMENTLIBRARIES = "DocumentLibraries";

        public const string TEXTPARTITIONING_DATAPIPELINESTAGE_PARITTIONINGSTRATETGY = "PartitioningStrategy";

        public const string GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGMODEL = "EmbeddingModel";
        public const string GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS = "EmbeddingDimensions";

        public const string AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORDATABASEOBJECTID = "VectorDatabaseObjectId";
        public const string AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREOBJECTID = "VectorStoreObjectId";
        public const string AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS = "EmbeddingDimensions";

        public const string IMAGE_CONTENTTEXTEXTRACTION_AIMODELOBJECTID = "AIModelObjectId";

        public const string IMAGE_METADATATEXTEXTRACTION_AIMODELOBJECTID = "AIModelObjectId";

        public const string TOKEN_CONTENTTEXTPARTITIONING_PARTITIONSIZETOKENS = "PartitionSizeTokens";
        public const string TOKEN_CONTENTTEXTPARTITIONING_PARTITIONOVERLAPTOKENS = "PartitionOverlapTokens";
    }
}
