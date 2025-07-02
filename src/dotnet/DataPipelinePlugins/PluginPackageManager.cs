using FoundationaLLM.Common.Constants.Plugins;
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.AZUREDATALAKE_DATASOURCE}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.CONTEXTFILE_DATASOURCE}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.SHAREPOINTONLINE_DATASOURCE}",
                    Name = PluginNames.SHAREPOINTONLINE_DATASOURCE,
                    DisplayName = "SharePoint Online Data Source (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for SharePoint Online data sources.",
                    Category = PluginCategoryNames.DataSource,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.SHAREPOINTONLINE_DATASOURCE_DOCUMENTLIBRARIES,
                            Type = PluginParameterTypes.Array,
                            Description = "List of strings defining SharePoint Online document libraries."
                        }
                    ],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.TEXTPARTITIONING_DATAPIPELINESTAGE}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE}",
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
                        },
                        new() {
                            Name = PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_EMBEDDINGDIMENSIONS,
                            Type = PluginParameterTypes.Int,
                            Description = "The number of dimensions used for embedding."
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE}",
                    Name = PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE,
                    DisplayName = "Knowledge Extraction Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for knowledge extraction data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONPROMPTOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Prompt resource that is used for entity extraction."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONCOMPLETIONMODEL,
                            Type = PluginParameterTypes.String,
                            Description = "The completion model used for entity extraction."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONENTITYTYPES,
                            Type = PluginParameterTypes.String,
                            Description = "List of comma-separated entity types to extract (e.g., PERSON, ORGANIZATION, LOCATION)."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMODELTEMPERATURE,
                            Type = PluginParameterTypes.Float,
                            Description = "The temperature used for the entity extraction model (0.0 for deterministic output, 1.0 for more creative output)."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONMAXOUTPUTTOKENCOUNT,
                            Type = PluginParameterTypes.Int,
                            Description = "The maximum number of output tokens for the entity extraction model."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE_ENTITYEXTRACTIONPROMPTOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Prompt/prompts",
                                FilterActionPayload = new {
                                    category = "DataPipeline"
                                }
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE}",
                    Name = PluginNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE,
                    DisplayName = "Knowledge Graph Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for knowledge graph data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONPROMPTOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Prompt resource that is used for entity summarization."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODEL,
                            Type = PluginParameterTypes.String,
                            Description = "The completion model used for entity summarization."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMODELTEMPERATURE,
                            Type = PluginParameterTypes.Float,
                            Description = "The temperature used for the entity summarization model (recommended value is 0.7)."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONCOMPLETIONMAXOUTPUTTOKENCOUNT,
                            Type = PluginParameterTypes.Int,
                            Description = "The maximum number of output tokens for the entity summarization model."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONEMBEDDINGMODEL,
                            Type = PluginParameterTypes.String,
                            Description = "The embedding model used for entity summary embedding (the recommended model is text-embedding-3-large)."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONEMBEDDINGDIMENSIONS,
                            Type = PluginParameterTypes.Int,
                            Description = "The number of dimensions used for embedding entity summarizations (the recommended number is 2048)."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEGRAPHID,
                            Type = PluginParameterTypes.String,
                            Description = "The FoundationaLLM resource identifier of the Knowledge Graph resource that identifies the knowledge graph to be built."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_ENTITYSUMMARIZATIONPROMPTOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Prompt/prompts",
                                FilterActionPayload = new {
                                    category = "DataPipeline"
                                }
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PDF_CONTENTTEXTEXTRACTION}",
                    Name = PluginNames.PDF_CONTENTTEXTEXTRACTION,
                    DisplayName = "PDF Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from PDF files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "PDF",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.DOCX_CONTENTTEXTEXTRACTION}",
                    Name = PluginNames.DOCX_CONTENTTEXTEXTRACTION,
                    DisplayName = "DOCX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from DOCX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "DOCX",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.PPTX_CONTENTTEXTEXTRACTION}",
                    Name = PluginNames.PPTX_CONTENTTEXTEXTRACTION,
                    DisplayName = "PPTX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from PPTX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "PPTX",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.XLSX_CONTENTTEXTEXTRACTION}",
                    Name = PluginNames.XLSX_CONTENTTEXTEXTRACTION,
                    DisplayName = "XLSX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from XLSX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Subcategory = "XLSX",
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.IMAGE_CONTENTTEXTEXTRACTION}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.IMAGE_METADATATEXTEXTRACTION}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.TOKEN_CONTENTTEXTPARTITIONING}",
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.SEMANTIC_CONTENTTEXTPARTITIONING}",
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
            PluginNames.SHAREPOINTONLINE_DATASOURCE => new SharePointOnlineDataSourcePlugin(dataSourceObjectId, pluginParameters, this, serviceProvider),
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
            PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE => new KnowledgeExtractionDataPipelineStagePlugin(pluginParameters, this, serviceProvider),
            PluginNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE => new KnowledgeGraphDataPipelineStagePlugin(pluginParameters, this, serviceProvider),
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
