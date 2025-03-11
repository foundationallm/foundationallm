using FoundationaLLM.Common.Constants.Plugins;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Plugins.Metadata;

namespace FoundationaLLM.Plugins.DataPipeline
{
    /// <summary>
    /// The plugin package manager.
    /// </summary>
    public class PluginPackageManager : IPluginPackageManager
    {
        private const string PACKAGE_NAME = "Dotnet-FoundationaLLMDataPipelinePlugins";

        /// <inheritdoc/>
        public PluginPackageMetadata GetMetadata(string instanceId) => new()
        {
            Name = PACKAGE_NAME,
            DisplayName = "FoundationaLLM Data Pipeline Plugins (.NET)",
            Description = "The FoundationaLLM Data Pipeline plugins package for .NET.",
            Platform = PluginPackagePlatform.Dotnet,
            Plugins = [
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-AzureDataLakeDataSource",
                    Name = $"{PACKAGE_NAME}-AzureDataLakeDataSource",
                    DisplayName = "Azure Data Lake Data Source (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for Azure Data Lake data sources.",
                    Category = PluginCategoryNames.DataSource,
                    Parameters = [
                        new() {
                            Name = "Folders",
                            Type = PluginParameterTypes.Array,
                            Description = "List of strings defining data lake folders (the first part identifies the container name)."
                        }
                    ],
                    Dependencies = [
                        new() {
                            SelectionType = PluginDependencySelectionTypes.Multiple,
                            DependencyPluginNames = [
                                $"{PACKAGE_NAME}-PDFContentTextExtraction",
                                $"{PACKAGE_NAME}-DOCXContentTextExtraction",
                                $"{PACKAGE_NAME}-PPTXContentTextExtraction",
                                $"{PACKAGE_NAME}-XSLXContentTextExtraction",
                                $"{PACKAGE_NAME}-ImageContentTextExtraction",
                                $"{PACKAGE_NAME}-ImageMetadataTextExtraction"
                            ]
                        }
                    ]
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-TextExtractionDataPipelineStage",
                    Name = $"{PACKAGE_NAME}-TextExtractionDataPipelineStage",
                    DisplayName = "Text Extraction Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text extraction data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-TextPartitioningDataPipelineStage",
                    Name = $"{PACKAGE_NAME}-TextPartitioningDataPipelineStage",
                    DisplayName = "Text Partitioning Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text partitioning data pipeline stages.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = "PartitioningStrategy",
                            Type = PluginParameterTypes.String,
                            Description = "Strategy used to partition text (can be Token or Semantic)."
                        }
                    ],
                    Dependencies = [
                        new() {
                            SelectionType = PluginDependencySelectionTypes.Single,
                            DependencyPluginNames = [
                                $"{PACKAGE_NAME}-TokenContentTextPartitioning",
                                $"{PACKAGE_NAME}-SemanticContentTextPartitioning"
                            ]
                        }
                    ]
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-GatewayTextEmbeddingDataPipelineStage",
                    Name = $"{PACKAGE_NAME}-GatewayTextEmbeddingDataPipelineStage",
                    DisplayName = "Gateway Text Embedding Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text emebdding data pipeline stages that perform text embedding using the FoundationaLLM Gateway API.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = "EmbeddingModel",
                            Type = PluginParameterTypes.String,
                            Description = "The embedding model used for embedding (the recommended model is text-embedding-3-large)."
                        },
                        new() {
                            Name = "EmbeddingDimensions",
                            Type = PluginParameterTypes.Int,
                            Description = "The number of dimensions used for embedding (the recommended number is 2048)."
                        }
                    ],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-AzureAISearchIndexingDataPipelineStage",
                    Name = $"{PACKAGE_NAME}-AzureAISearchIndexingDataPipelineStage",
                    DisplayName = "Azure AI Search Indexing Data Pipeline Stage (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for indexing data pipeline stages that use Azure AI Search.",
                    Category = PluginCategoryNames.DataPipelineStage,
                    Parameters = [
                        new() {
                            Name = "APIEndpointConfigurationObjectId",
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the API Endpoint Configuration resource that identifies the Azure AI Search instance."
                        },
                        new() {
                            Name = "IndexName",
                            Type = PluginParameterTypes.String,
                            Description = "The name of the Azure AI Search index."
                        },
                        new() {
                            Name = "IndexPartitionName",
                            Type = PluginParameterTypes.String,
                            Description = "The name of the Azure AI Search index partition (to be added as a metadata entry and used for logical separation within a given physical index)."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            "APIEndpointConfigurationObjectId",
                            new() {
                                ResourcePath = "providers/FoundationaLLM.Configuration/apiEndpointConfigurations",
                                FilterActionPayload = new {
                                    Category = "General",
                                    Subcategory = "Indexing"
                                }
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-PDFContentTextExtraction",
                    Name = $"{PACKAGE_NAME}-PDFContentTextExtraction",
                    DisplayName = "PDF Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from PDF files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-DOCXContentTextExtraction",
                    Name = $"{PACKAGE_NAME}-DOCXContentTextExtraction",
                    DisplayName = "DOCX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from DOCX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-PPTXContentTextExtraction",
                    Name = $"{PACKAGE_NAME}-PPTXContentTextExtraction",
                    DisplayName = "PPTX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from PPTX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-XSLXContentTextExtraction",
                    Name = $"{PACKAGE_NAME}-XSLXContentTextExtraction",
                    DisplayName = "XSLX Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text extraction from XSLX files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Parameters = [],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-ImageContentTextExtraction",
                    Name = $"{PACKAGE_NAME}-ImageContentTextExtraction",
                    DisplayName = "Image Content Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for text extraction from image files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Parameters = [
                        new() {
                            Name = "AIModelObjectId",
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the AI Model resource that identifies the LLM model used to extract text from images."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            "AIModelObjectId",
                            new() {
                                ResourcePath = "providers/FoundationaLLM.AIModel/aiModels",
                                FilterActionPayload = null
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-ImageMetadataTextExtraction",
                    Name = $"{PACKAGE_NAME}-ImageMetadataContentTextExtraction",
                    DisplayName = "Image Metadata Text Extraction (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for metadata (description) extraction from image files.",
                    Category = PluginCategoryNames.ContentTextExtraction,
                    Parameters = [
                        new() {
                            Name = "AIModelObjectId",
                            Type = PluginParameterTypes.ResourceObjectId,
                            Description = "The FoundationaLLM resource identifier of the AI Model resource that identifies the LLM model used to extract metadata (description) from images."
                        }
                    ],
                    ParameterSelectionHints = new() {
                        {
                            "AIModelObjectId",
                            new() {
                                ResourcePath = "providers/FoundationaLLM.AIModel/aiModels",
                                FilterActionPayload = null
                            }
                        }
                    },
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-TokenContentTextPartitioning",
                    Name = $"{PACKAGE_NAME}-TokenContentTextPartitioning",
                    DisplayName = "Token Content Text Partitioning (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text partitioning using a token-based strategy.",
                    Category = PluginCategoryNames.ContentTextPartitioning,
                    Parameters = [
                        new() {
                            Name = "PartitionSizeTokens",
                            Type = PluginParameterTypes.Int,
                            Description = "The size of the text partition in tokens."
                        },
                        new() {
                            Name = "PartitionOverlapTokens",
                            Type = PluginParameterTypes.Int,
                            Description = "The size of the text partition overlaps in tokens."
                        }
                    ],
                    Dependencies = []
                },
                new() {
                    ObjectId = $"instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PACKAGE_NAME}-SemanticContentTextPartitioning",
                    Name = $"{PACKAGE_NAME}-SemanticContentTextPartitioning",
                    DisplayName = "Semantic Content Text Partitioning (FoundationaLLM)",
                    Description = "Provides the FoundationaLLM standard implementation for content text partitioning using a semantic strategy.",
                    Category = PluginCategoryNames.ContentTextPartitioning,
                    Parameters = [],
                    Dependencies = []
                }
            ]
        };
    }
}
