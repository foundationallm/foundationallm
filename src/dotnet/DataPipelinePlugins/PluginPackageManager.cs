using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.Plugins.Metadata;
using FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextExtraction;
using FoundationaLLM.Plugins.DataPipeline.Plugins.ContentTextPartitioning;
using FoundationaLLM.Plugins.DataPipeline.Plugins.DataPipelineStage;
using FoundationaLLM.Plugins.DataPipeline.Plugins.DataSource;
using System.Collections.Concurrent;

namespace FoundationaLLM.Plugins.DataPipeline
{
    /// <summary>
    /// The plugin package manager.
    /// </summary>
    public class PluginPackageManager : IPluginPackageManager
    {
        private readonly ConcurrentDictionary<string, object> _services = [];

        /// <inheritdoc/>
        public bool TryGetService(string serviceName, out object? service) =>
            _services.TryGetValue(serviceName, out service);

        /// <inheritdoc/>
        public void RegisterService(string serviceName, object service) =>
            _services.TryAdd(serviceName, service);

        /// <inheritdoc/>
        public PluginPackageMetadata GetMetadata(string instanceId) => new()
        {
            Name = PluginNames.PACKAGE_NAME,
            DisplayName = "FoundationaLLM Data Pipeline Plugins (.NET)",
            Description = "The FoundationaLLM Data Pipeline plugins package for .NET.",
            Platform = PluginPackagePlatform.Dotnet,
            Plugins = [
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-AzureDataLakeDataSource",
                    Name = PluginNames.AZUREDATALAKE_DATASOURCE,
                    DisplayName = "Azure Data Lake Data Source (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for Azure Data Lake data sources.",
                    Category = PluginCategoryNames.DataSource,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.AZUREDATALAKE_DATASOURCE_FOLDERS,
                            Type = PluginParameterTypes.Array,
                            Description = "List of strings defining data lake folders (the first part identifies the container name)."
                        }
                    ],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-ContextFileDataSource",
                    Name = PluginNames.CONTEXTFILE_DATASOURCE,
                    DisplayName = "Context File Data Source (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for Context File data sources.",
                    Category = PluginCategoryNames.DataSource,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTEXTFILEOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM object identifier of the context file."
                        }
                    ],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-TextExtractionDataPipelineStage",
                    Name = PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE,
                    DisplayName = "Text Extraction Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text extraction data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [],
                    Dependencies = [
                        new() {
                            SelectionType = PluginDependencySelectionTypes.Multiple,
                            DependencyPluginNames = [
                                PluginNames.PDF_CONTENTTEXTEXTRACTION,
                                PluginNames.DOCX_CONTENTTEXTEXTRACTION,
                                PluginNames.PPTX_CONTENTTEXTEXTRACTION,
                                PluginNames.XLSX_CONTENTTEXTEXTRACTION,
                                PluginNames.IMAGE_CONTENTTEXTEXTRACTION,
                                PluginNames.IMAGE_METADATATEXTEXTRACTION
                            ]
                        }
                    ]
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-TextPartitioningDataPipelineStage",
                    Name = PluginNames.TEXTPARTITIONING_DATAPIPELINESTAGE,
                    DisplayName = "Text Partitioning Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text partitioning data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.TEXTPARTITIONING_DATAPIPELINESTAGE_PARITTIONINGSTRATETGY,
                            Type = PluginParameterTypes.String,
                            Description = "Strategy used to partition text (can be Token or Semantic)."
                        }
                    ],
                    Dependencies = [
                        new() {
                            SelectionType = PluginDependencySelectionTypes.Single,
                            DependencyPluginNames = [
                                PluginNames.TOKEN_CONTENTTEXTPARTITIONING,
                                PluginNames.SEMANTIC_CONTENTTEXTPARTITIONING
                            ]
                        }
                    ]
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-GatewayTextEmbeddingDataPipelineStage",
                    Name = PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE,
                    DisplayName = "Gateway Text Embedding Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text emebdding data pipeline stages that perform text embedding using the FoundationaLLM Gateway API.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGMODEL,
                            Type = PluginParameterTypes.String,
                            Description = "The embedding model used for embedding (the recommended model is text-embedding-3-large)."
                        },
                        new() {
                            Name = PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS,
                            Type = PluginParameterTypes.Int,
                            Description = "The number of dimensions used for embedding (the recommended number is 2048)."
                        }
                    ],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-AzureAISearchIndexingDataPipelineStage",
                    Name = PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE,
                    DisplayName = "Azure AI Search Indexing Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for indexing data pipeline stages that use Azure AI Search.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORDATABASEOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Vector Database resource that identifies the Azure AI Search instance and the index."
                        },
                        new() {
                            Name = PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Vector Store resource that identifies the logical partition of the Azure AI Search index."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORDATABASEOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Vectorization/vectorDatabases",
                                FilterActionPayload = new {
                                    category = "AzureAISearch"
                                }
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-PDFContentTextExtraction",
                    Name = PluginNames.PDF_CONTENTTEXTEXTRACTION,
                    DisplayName = "PDF Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from PDF files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "PDF",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-DOCXContentTextExtraction",
                    Name = PluginNames.DOCX_CONTENTTEXTEXTRACTION,
                    DisplayName = "DOCX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from DOCX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "DOCX",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-PPTXContentTextExtraction",
                    Name = PluginNames.PPTX_CONTENTTEXTEXTRACTION,
                    DisplayName = "PPTX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from PPTX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "PPTX",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-XLSXContentTextExtraction",
                    Name = PluginNames.XLSX_CONTENTTEXTEXTRACTION,
                    DisplayName = "XLSX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from XLSX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "XLSX",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-ImageContentTextExtraction",
                    Name = PluginNames.IMAGE_CONTENTTEXTEXTRACTION,
                    DisplayName = "Image Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text extraction from image files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "JPG,JPEG,GIF,PNG,WEBP,TIFF,BMP",
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.IMAGE_CONTENTTEXTEXTRACTION_AIMODELOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the AI Model resource that identifies the LLM model used to extract text from images."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.IMAGE_CONTENTTEXTEXTRACTION_AIMODELOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.AIModel/aiModels",
                                FilterActionPayload = null
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-ImageMetadataTextExtraction",
                    Name = PluginNames.IMAGE_METADATATEXTEXTRACTION,
                    DisplayName = "Image Metadata Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for metadata (description) extraction from image files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "JPG,JPEG,GIF,PNG,WEBP,TIFF,BMP",
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.IMAGE_METADATATEXTEXTRACTION_AIMODELOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the AI Model resource that identifies the LLM model used to extract metadata (description) from images."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.IMAGE_METADATATEXTEXTRACTION_AIMODELOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.AIModel/aiModels",
                                FilterActionPayload = null
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-TokenContentTextPartitioning",
                    Name = PluginNames.TOKEN_CONTENTTEXTPARTITIONING,
                    DisplayName = "Token Content Text Partitioning (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text partitioning using a token-based strategy.",
                    Category = PluginCategoryNames.ContentTextPartitioning,
                    Subcategory = "Token",
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONSIZETOKENS,
                            Type = PluginParameterTypes.Int,
                            Description = "The size of the text partition in tokens."
                        },
                        new() {
                            Name = PluginParameterNames.TOKEN_CONTENTTEXTPARTITIONING_PARTITIONOVERLAPTOKENS,
                            Type = PluginParameterTypes.Int,
                            Description = "The size of the text partition overlaps in tokens."
                        }
                    ],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PACKAGE_NAME}-SemanticContentTextPartitioning",
                    Name = PluginNames.SEMANTIC_CONTENTTEXTPARTITIONING,
                    DisplayName = "Semantic Content Text Partitioning (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text partitioning using a semantic strategy.",
                    Category = PluginCategoryNames.ContentTextPartitioning,
                    Subcategory = "Semantic",
                    Parameters = [],
                    Dependencies = []
                }
            ]
        };

        /// <inheritdoc/>
        public IDataSourcePlugin GetDataSourcePlugin(
            string pluginName,
            string dataSourceObjectId,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.AZUREDATALAKE_DATASOURCE => new AzureDataLakeDataSourcePlugin(dataSourceObjectId, pluginParameters, this, serviceProvider),
            PluginNames.CONTEXTFILE_DATASOURCE => new ContextFileDataSourcePlugin(dataSourceObjectId, pluginParameters, this, serviceProvider),
            _ => throw new NotImplementedException($"The data source plugin '{pluginName}' is not implemented.")
        };

        /// <inheritdoc/>
        public IDataPipelineStagePlugin GetDataPipelineStagePlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE => new TextExtractionDataPipelineStagePlugin(pluginParameters, this, serviceProvider),
            PluginNames.TEXTPARTITIONING_DATAPIPELINESTAGE => new TextPartitioningDataPipelineStagePlugin(pluginParameters, this, serviceProvider),
            PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE => new GatewayTextEmbeddingDataPipelineStagePlugin(pluginParameters, this, serviceProvider),
            PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE => new AzureAIIndexingDataPipelineStagePlugin(pluginParameters, this, serviceProvider),
            _ => throw new NotImplementedException($"The data pipeline stage plugin '{pluginName}' is not implemented.")
        };

        /// <inheritdoc/>
        public IContentTextExtractionPlugin GetContentTextExtractionPlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.PDF_CONTENTTEXTEXTRACTION => new PDFContentTextExtractionPlugin(pluginParameters, this, serviceProvider),
            PluginNames.DOCX_CONTENTTEXTEXTRACTION => new DOCXContentTextExtractionPlugin(pluginParameters, this, serviceProvider),
            PluginNames.PPTX_CONTENTTEXTEXTRACTION => new PPTXContentTextExtractionPlugin(pluginParameters, this, serviceProvider),
            PluginNames.XLSX_CONTENTTEXTEXTRACTION => new XLSXContentTextExtractionPlugin(pluginParameters, this, serviceProvider),
            PluginNames.IMAGE_CONTENTTEXTEXTRACTION => new ImageContentTextExtractionPlugin(pluginParameters, this, serviceProvider),
            PluginNames.IMAGE_METADATATEXTEXTRACTION => new ImageMetadataTextExtractionPlugin(pluginParameters, this, serviceProvider),
            _ => throw new NotImplementedException($"The content text extraction plugin '{pluginName}' is not implemented.")
        };

        /// <inheritdoc/>
        public IContentTextPartitioningPlugin GetContentTextPartitioningPlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.TOKEN_CONTENTTEXTPARTITIONING => new TokenContentTextPartitioningPlugin(pluginParameters, this, serviceProvider),
            PluginNames.SEMANTIC_CONTENTTEXTPARTITIONING => new SemanticContentTextPartitioningPlugin(pluginParameters, this, serviceProvider),
            _ => throw new NotImplementedException($"The content text partitioning plugin '{pluginName}' is not implemented.")
        };
    }
}
