namespace FoundationaLLM.Plugins.DataPipeline
{
    /// <summary>
    /// Provides the names of the plugins provided by the package.
    /// </summary>
    public static class PluginNames
    {
        public const string PACKAGE_NAME = "Dotnet-FoundationaLLMDataPipelinePlugins";

        public const string AZUREDATALAKE_DATASOURCE = $"{PACKAGE_NAME}-AzureDataLakeDataSource";
        public const string CONTEXTFILE_DATASOURCE = $"{PACKAGE_NAME}-ContextFileDataSource";
        public const string SHAREPOINTONLINE_DATASOURCE = $"{PACKAGE_NAME}-SharePointOnlineDataSource";
        public const string TEXTEXTRACTION_DATAPIPELINESTAGE = $"{PACKAGE_NAME}-TextExtractionDataPipelineStage";
        public const string TEXTPARTITIONING_DATAPIPELINESTAGE = $"{PACKAGE_NAME}-TextPartitioningDataPipelineStage";
        public const string GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE = $"{PACKAGE_NAME}-GatewayTextEmbeddingDataPipelineStage";
        public const string AZUREAISEARCHINDEXING_DATAPIPELINESTAGE = $"{PACKAGE_NAME}-AzureAISearchIndexingDataPipelineStage";
        public const string AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE = $"{PACKAGE_NAME}-AzureAISearchRemovalDataPipelineStage";
        public const string PDF_CONTENTTEXTEXTRACTION = $"{PACKAGE_NAME}-PDFContentTextExtraction";
        public const string DOCX_CONTENTTEXTEXTRACTION = $"{PACKAGE_NAME}-DOCXContentTextExtraction";
        public const string PPTX_CONTENTTEXTEXTRACTION = $"{PACKAGE_NAME}-PPTXContentTextExtraction";
        public const string XLSX_CONTENTTEXTEXTRACTION = $"{PACKAGE_NAME}-XLSXContentTextExtraction";
        public const string IMAGE_CONTENTTEXTEXTRACTION = $"{PACKAGE_NAME}-ImageContentTextExtraction";
        public const string IMAGE_METADATATEXTEXTRACTION = $"{PACKAGE_NAME}-ImageMetadataTextExtraction";
        public const string TOKEN_CONTENTTEXTPARTITIONING = $"{PACKAGE_NAME}-TokenContentTextPartitioning";
        public const string SEMANTIC_CONTENTTEXTPARTITIONING = $"{PACKAGE_NAME}-SemanticContentTextPartitioning";
        public const string KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE = $"{PACKAGE_NAME}-KnowledgeExtractionDataPipelineStage";
        public const string KNOWLEDGEGRAPH_DATAPIPELINESTAGE = $"{PACKAGE_NAME}-KnowledgeGraphDataPipelineStage";
    }
}
