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
                        },
                        new() {
                            Name = PluginParameterNames.CONTEXTFILE_DATASOURCE_CONTENTACTION,
                            Type = PluginParameterTypes.String,
                            Description = "The action to be performed on the content item. Can be 'add-or-update' or 'remove'."
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
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.TEXTEXTRACTION_DATAPIPELINESTAGE_MAXCONTENTSIZECHARACTERS,
                            Type = PluginParameterTypes.Int,
                            Description = "The maximum allowed content item size in characters."
                        }],
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.AZUREAICONTENTSAFETYSHIELDING_DATAPIPELINESTAGE}",
                    Name = PluginNames.AZUREAICONTENTSAFETYSHIELDING_DATAPIPELINESTAGE,
                    DisplayName = "Azure AI Content Safety Shielding Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for cotent shielding data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE}",
                    Name = PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE,
                    DisplayName = "Gateway Text Embedding Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text emebdding data pipeline stages that perform text embedding using the FoundationaLLM Gateway API.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Knowledge Unit that provides the embedding configuration."
                        },
                        new() {
                            Name = PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_EMBEDDINGREQUESTSIZETOKENS,
                            Type = PluginParameterTypes.Int,
                            Description = "The maximum size in tokens of each embedding request sent to the Gateway API."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Context/knowledgeUnits",
                                FilterActionPayload = null
                            }
                        }
                    },
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
                            Name = PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Knowledge Unit that provides the embedding configuration."
                        },
                        new() {
                            Name = PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_VECTORSTOREID,
                            Type = PluginParameterTypes.String,
                            Description = "The name of the Vector Store resource that identifies the logical partition of the Azure AI Search index."
                        },
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Context/knowledgeUnits",
                                FilterActionPayload = null
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE}",
                    Name = PluginNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE,
                    DisplayName = "Azure AI Search Removal Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content removal data pipeline stages that use Azure AI Search.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Knowledge Unit that provides the vector database configuration."
                        },
                        new() {
                            Name = PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_VECTORSTOREID,
                            Type = PluginParameterTypes.String,
                            Description = "The name of the Vector Store resource that identifies the logical partition of the Azure AI Search index."
                        },
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Context/knowledgeUnits",
                                FilterActionPayload = null
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.KNOWLEDGEGRAPH_CONSOLIDATION_DATAPIPELINESTAGE}",
                    Name = PluginNames.KNOWLEDGEGRAPH_CONSOLIDATION_DATAPIPELINESTAGE,
                    DisplayName = "Knowledge Graph Consolidation Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for knowledge graph consolidation data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.KNOWLEDGEGRAPH_SUMMARIZATION_DATAPIPELINESTAGE}",
                    Name = PluginNames.KNOWLEDGEGRAPH_SUMMARIZATION_DATAPIPELINESTAGE,
                    DisplayName = "Knowledge Graph Summarization Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for knowledge graph summarization data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONPROMPTOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Prompt resource that is used for entity summarization."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMODEL,
                            Type = PluginParameterTypes.String,
                            Description = "The completion model used for entity summarization."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMODELTEMPERATURE,
                            Type = PluginParameterTypes.Float,
                            Description = "The temperature used for the entity summarization model (recommended value is 0.7)."
                        },
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONCOMPLETIONMAXOUTPUTTOKENCOUNT,
                            Type = PluginParameterTypes.Int,
                            Description = "The maximum number of output tokens for the entity summarization model."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_SUMMARIZATIONPROMPTOBJECTID,
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
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.KNOWLEDGEGRAPH_EMBEDDING_DATAPIPELINESTAGE}",
                    Name = PluginNames.KNOWLEDGEGRAPH_EMBEDDING_DATAPIPELINESTAGE,
                    DisplayName = "Knowledge Graph Embedding Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for knowledge graph embedding data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Knowledge Unit that provides the embedding configuration."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Context/knowledgeUnits",
                                FilterActionPayload = null
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.KNOWLEDGEGRAPH_INDEXING_DATAPIPELINESTAGE}",
                    Name = PluginNames.KNOWLEDGEGRAPH_INDEXING_DATAPIPELINESTAGE,
                    DisplayName = "Knowledge Graph Indexing Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for knowledge graph indexing data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Knowledge Unit that provides the indexing configuration."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Context/knowledgeUnits",
                                FilterActionPayload = null
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.KNOWLEDGEGRAPH_PUBLISHING_DATAPIPELINESTAGE}",
                    Name = PluginNames.KNOWLEDGEGRAPH_PUBLISHING_DATAPIPELINESTAGE,
                    DisplayName = "Knowledge Graph Publishing Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for knowledge graph publishing data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the Knowledge Unit that provides the publishing configuration."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            PluginParameterNames.KNOWLEDGEGRAPH_DATAPIPELINESTAGE_KNOWLEDGEUNITOBJECTID,
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Context/knowledgeUnits",
                                FilterActionPayload = null
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
            IPluginPackageManagerResolver packageManagerResolver,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.AZUREDATALAKE_DATASOURCE => new AzureDataLakeDataSourcePlugin(
                dataSourceObjectId, pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.CONTEXTFILE_DATASOURCE => new ContextFileDataSourcePlugin(
                dataSourceObjectId, pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.SHAREPOINTONLINE_DATASOURCE => new SharePointOnlineDataSourcePlugin(
                dataSourceObjectId, pluginParameters, this, packageManagerResolver, serviceProvider),
            _ => throw new NotImplementedException($"The data source plugin '{pluginName}' is not implemented.")
        };

        /// <inheritdoc/>
        public IDataPipelineStagePlugin GetDataPipelineStagePlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IPluginPackageManagerResolver packageManagerResolver,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.TEXTEXTRACTION_DATAPIPELINESTAGE => new TextExtractionDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.TEXTPARTITIONING_DATAPIPELINESTAGE => new TextPartitioningDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.AZUREAICONTENTSAFETYSHIELDING_DATAPIPELINESTAGE => new AzureAIContentSafetyShieldingDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.GATEWAYTEXTEMBEDDING_DATAPIPELINESTAGE => new GatewayTextEmbeddingDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.AZUREAISEARCHINDEXING_DATAPIPELINESTAGE => new AzureAISearchIndexingDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.AZUREAISEARCHREMOVAL_DATAPIPELINESTAGE => new AzureAISearchRemovalDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.KNOWLEDGEEXTRACTION_DATAPIPELINESTAGE => new KnowledgeExtractionDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.KNOWLEDGEGRAPH_CONSOLIDATION_DATAPIPELINESTAGE => new KnowledgeGraphConsolidationDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.KNOWLEDGEGRAPH_SUMMARIZATION_DATAPIPELINESTAGE => new KnowledgeGraphSummarizationDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.KNOWLEDGEGRAPH_EMBEDDING_DATAPIPELINESTAGE => new KnowledgeGraphEmbeddingDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.KNOWLEDGEGRAPH_INDEXING_DATAPIPELINESTAGE => new KnowledgeGraphIndexingDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.KNOWLEDGEGRAPH_PUBLISHING_DATAPIPELINESTAGE => new KnowledgeGraphPublishingDataPipelineStagePlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            _ => throw new NotImplementedException($"The data pipeline stage plugin '{pluginName}' is not implemented.")
        };

        /// <inheritdoc/>
        public IContentTextExtractionPlugin GetContentTextExtractionPlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IPluginPackageManagerResolver packageManagerResolver,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.PDF_CONTENTTEXTEXTRACTION => new PDFContentTextExtractionPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.DOCX_CONTENTTEXTEXTRACTION => new DOCXContentTextExtractionPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.PPTX_CONTENTTEXTEXTRACTION => new PPTXContentTextExtractionPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.XLSX_CONTENTTEXTEXTRACTION => new XLSXContentTextExtractionPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.IMAGE_CONTENTTEXTEXTRACTION => new ImageContentTextExtractionPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.IMAGE_METADATATEXTEXTRACTION => new ImageMetadataTextExtractionPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            _ => throw new NotImplementedException($"The content text extraction plugin '{pluginName}' is not implemented.")
        };

        /// <inheritdoc/>
        public IContentTextPartitioningPlugin GetContentTextPartitioningPlugin(
            string pluginName,
            Dictionary<string, object> pluginParameters,
            IPluginPackageManagerResolver packageManagerResolver,
            IServiceProvider serviceProvider) => pluginName switch
        {
            PluginNames.TOKEN_CONTENTTEXTPARTITIONING => new TokenContentTextPartitioningPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            PluginNames.SEMANTIC_CONTENTTEXTPARTITIONING => new SemanticContentTextPartitioningPlugin(
                pluginParameters, this, packageManagerResolver, serviceProvider),
            _ => throw new NotImplementedException($"The content text partitioning plugin '{pluginName}' is not implemented.")
        };
    }
}
