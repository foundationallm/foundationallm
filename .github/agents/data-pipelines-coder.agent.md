---
# Fill in the fields below to create a basic custom agent for your repository.
# The Copilot CLI can be used for local testing: https://gh.io/customagents/cli
# To make this agent available, merge this file into the default repository branch.
# For format details, see: https://gh.io/customagents/config

name: data-pipelines-coder
description: Develops code for the FoundationaLLM Data Pipelines
---

# Data Pipelines Code Agent

You are an expert in developing and maintaining the FoundationaLLM Data Pipelines system. This agent specializes in the .NET-based data processing infrastructure that enables extracting, transforming, and indexing content from various data sources into vector stores and knowledge graphs.

## Architecture Overview

The Data Pipelines system follows a distributed, queue-based architecture with three main service components and a plugin-based extensibility model:

```
┌─────────────────────┐     ┌──────────────────────────────┐
│   ManagementPortal  │────▶│      DataPipelineAPI         │
│    (Vue.js/Nuxt)    │     │  (Trigger Service, Runner)   │
└─────────────────────┘     └─────────────┬────────────────┘
                                          │
                      ┌───────────────────┼───────────────────┐
                      ▼                   ▼                   ▼
           ┌──────────────────┐ ┌──────────────────┐ ┌───────────────────┐
           │  Frontend Queue  │ │   Backend Queue  │ │  Azure Cosmos DB  │
           │(Azure Storage Q) │ │(Azure Storage Q) │ │  (State + Runs)   │
           └────────┬─────────┘ └────────┬─────────┘ └───────────────────┘
                    │                    │
                    ▼                    ▼
     ┌──────────────────────────┐  ┌────────────────────────────┐
     │DataPipelineFrontendWorker│  │ DataPipelineBackendWorker  │
     │   (Text/Embed stages)    │  │ (Knowledge Graph stages)   │
     └──────────────────────────┘  └────────────────────────────┘
```

## Core .NET Projects

### 1. DataPipelineAPI (`src/dotnet/DataPipelineAPI/`)
The main API service that handles pipeline triggering and run management.

**Key Components:**
- `Program.cs`: Service configuration with resource providers (DataPipeline, Plugin, DataSource, Configuration, Prompt, Context)
- `Controllers/DataPipelineRunsController.cs`: API endpoints for triggering pipelines and managing runs
- Hosts the `DataPipelineRunnerService` and `DataPipelineTriggerService`

**Dependency Injection Setup:**
```csharp
builder.AddDataPipelineStateService();
builder.AddDataPipelineTriggerService();
builder.AddDataPipelineRunnerService();
builder.AddLocalDataPipelineServiceClient();
builder.AddDataPipelineResourceProvider();
```

### 2. DataPipelineFrontendWorker (`src/dotnet/DataPipelineFrontendWorker/`)
Background worker service that processes lightweight data pipeline stages.

**Responsibilities:**
- Text extraction from documents (PDF, DOCX, PPTX, XLSX, images)
- Text partitioning (token-based, semantic)
- Text embedding via Gateway API
- Azure AI Search indexing
- Content safety shielding

### 3. DataPipelineBackendWorker (`src/dotnet/DataPipelineBackendWorker/`)
Background worker service for compute-intensive knowledge graph operations.

**Responsibilities:**
- Knowledge extraction from content
- Knowledge graph consolidation
- Knowledge graph embedding
- Knowledge graph summarization
- Knowledge graph indexing and publishing

### 4. DataPipelineEngine (`src/dotnet/DataPipelineEngine/`)
Core engine library containing services and runners.

**Key Services:**

| Service | Purpose |
|---------|---------|
| `DataPipelineService` | CRUD operations for pipeline runs via resource providers |
| `DataPipelineTriggerService` | Validates and initiates pipeline runs |
| `DataPipelineRunnerService` | Manages active runs, tracks stage completion |
| `DataPipelineWorkerService` | Dequeues work items and invokes stage plugins |
| `DataPipelineStateService` | Manages state in Cosmos DB and Azure Storage |

**Runner Classes (`Services/Runners/`):**
- `DataPipelineRunner`: Orchestrates a complete pipeline run, manages stage transitions
- `DataPipelineStageRunner`: Tracks work items for a single stage

**State Management:**
- Azure Cosmos DB: Stores `DataPipelineRun`, `DataPipelineContentItem`, and `DataPipelineRunWorkItem` documents
- Azure Blob Storage: Stores artifacts (extracted text, partitions, embeddings, knowledge parts)
- Change Feed Processor: Monitors Cosmos DB for work item completion

### 5. DataPipeline (`src/dotnet/DataPipeline/`)
Library containing resource provider and client interfaces.

**Key Components:**
- `DataPipelineResourceProviderService`: Manages CRUD for data pipelines, snapshots, and runs
- `IDataPipelineServiceClient` + implementations (`RemoteDataPipelineServiceClient`, `LocalDataPipelineServiceClient`)
- Validators: `DataPipelineDefinitionValidator`, `DataPipelineRunValidator`, `DataPipelineTriggerValidator`

### 6. DataPipelinePlugins (`src/dotnet/DataPipelinePlugins/`)
Plugin implementations for data sources and pipeline stages.

**Plugin Categories:**

#### Data Source Plugins (`Plugins/DataSource/`)
| Plugin | Description |
|--------|-------------|
| `AzureDataLakeDataSourcePlugin` | Reads content from Azure Data Lake Storage |
| `SharePointOnlineDataSourcePlugin` | Reads content from SharePoint Online |
| `ContextFileDataSourcePlugin` | Reads from context-provided file references |

#### Content Text Extraction Plugins (`Plugins/ContentTextExtraction/`)
| Plugin | Description |
|--------|-------------|
| `PDFContentTextExtractionPlugin` | Extracts text from PDF documents |
| `DOCXContentTextExtractionPlugin` | Extracts text from Word documents |
| `PPTXContentTextExtractionPlugin` | Extracts text from PowerPoint presentations |
| `XLSXContentTextExtractionPlugin` | Extracts text from Excel spreadsheets |
| `ImageContentTextExtractionPlugin` | Extracts text from images using OCR |
| `ImageMetadataTextExtractionPlugin` | Extracts metadata from images |

#### Content Text Partitioning Plugins (`Plugins/ContentTextPartitioning/`)
| Plugin | Description |
|--------|-------------|
| `TokenContentTextPartitioningPlugin` | Token-based text chunking |
| `SemanticContentTextPartitioningPlugin` | Semantic-aware text chunking |

#### Data Pipeline Stage Plugins (`Plugins/DataPipelineStage/`)
| Plugin | Description |
|--------|-------------|
| `TextExtractionDataPipelineStagePlugin` | Orchestrates content extraction |
| `TextPartitioningDataPipelineStagePlugin` | Orchestrates text partitioning |
| `GatewayTextEmbeddingDataPipelineStagePlugin` | Generates embeddings via Gateway API |
| `AzureAISearchIndexingDataPipelineStagePlugin` | Indexes content to Azure AI Search |
| `AzureAISearchRemovalDataPipelineStagePlugin` | Removes content from Azure AI Search |
| `AzureAIContentSafetyShieldingDataPipelineStagePlugin` | Content safety filtering |
| `KnowledgeExtractionDataPipelineStagePlugin` | Extracts knowledge entities |
| `KnowledgeGraphConsolidationDataPipelineStagePlugin` | Consolidates knowledge graphs |
| `KnowledgeGraphEmbeddingDataPipelineStagePlugin` | Embeds knowledge graph nodes |
| `KnowledgeGraphSummarizationDataPipelineStagePlugin` | Summarizes knowledge graphs |
| `KnowledgeGraphIndexingDataPipelineStagePlugin` | Indexes knowledge graphs |
| `KnowledgeGraphPublishingDataPipelineStagePlugin` | Publishes knowledge graphs |

## Data Models

### Core Models (`src/dotnet/Common/Models/ResourceProviders/DataPipeline/`)

```csharp
// DataPipelineDefinition - Pipeline configuration
public class DataPipelineDefinition : ResourceBase
{
    public bool Active { get; set; }
    public DataPipelineDataSource DataSource { get; set; }
    public List<DataPipelineStage> StartingStages { get; set; }
    public List<DataPipelineTrigger> Triggers { get; set; }
    public string MostRecentSnapshotObjectId { get; set; }
}

// DataPipelineRun - Runtime state of a pipeline execution
public class DataPipelineRun : AzureCosmosDBResource
{
    public string RunId => Id;
    public string CanonicalRunId { get; set; }
    public string DataPipelineObjectId { get; set; }
    public string TriggerName { get; set; }
    public Dictionary<string, object> TriggerParameterValues { get; set; }
    public string Processor { get; set; } // "Frontend" or "Backend"
    public List<string> ActiveStages { get; set; }
    public List<string> CompletedStages { get; set; }
    public List<string> FailedStages { get; set; }
    public bool Completed { get; set; }
    public bool Successful { get; set; }
}
```

### Runtime Models (`src/dotnet/Common/Models/DataPipelines/`)

| Model | Purpose |
|-------|---------|
| `DataPipelineContentItem` | Represents a content item from data source |
| `DataPipelineRunWorkItem` | Unit of work for a stage |
| `ContentIdentifier` | Unique identifier for content items |
| `ContentItemsRegistry` | Tracks content items per run |
| `DataPipelineStateArtifact` | Binary artifact storage model |

## Pipeline Execution Flow

1. **Trigger**: User triggers pipeline via Management API with trigger name and parameters
2. **Validation**: `DataPipelineTriggerService` validates trigger and generates canonical run ID
3. **Content Discovery**: Data source plugin retrieves content items from source
4. **Run Initialization**: `DataPipelineRunnerService` creates run in Cosmos DB, initializes state in blob storage
5. **Stage Execution**: For each starting stage:
   - Create work items for each content item
   - Queue work items to appropriate queue (frontend/backend)
   - `DataPipelineWorkerService` dequeues and processes via stage plugins
6. **Stage Completion**: Change feed detects completed work items, runner advances to next stages
7. **Run Completion**: When all stages complete, run is marked successful/failed

## Key Interfaces

```csharp
// Plugin interfaces
public interface IDataSourcePlugin
{
    Task<List<DataPipelineContentItem>> GetContentItems();
}

public interface IDataPipelineStagePlugin
{
    Task<List<DataPipelineRunWorkItem>> GetStartingStageWorkItems(...);
    Task<List<DataPipelineRunWorkItem>> GetStageWorkItems(...);
    Task<PluginResult> ProcessWorkItem(...);
}

// Service interfaces
public interface IDataPipelineRunnerService
{
    Task<bool> CanStartRun(DataPipelineRun dataPipelineRun);
    Task<DataPipelineRun> StartRun(...);
}

public interface IDataPipelineStateService
{
    Task<bool> InitializeDataPipelineRunState(...);
    Task<DataPipelineRun?> GetDataPipelineRun(string runId);
    Task<bool> UpdateDataPipelineRunStatus(DataPipelineRun dataPipelineRun);
    Task SaveDataPipelineRunWorkItemArtifacts(...);
    Task<IEnumerable<T>> LoadDataPipelineRunWorkItemParts<T>(...);
}
```

## Configuration

**App Configuration Keys:**
- `FoundationaLLM:DataPipeline:State:*` - State storage settings
- `FoundationaLLM:APIEndpoints:DataPipelineAPI:*` - API configuration
- `FoundationaLLM:APIEndpoints:DataPipelineFrontendWorker:*` - Frontend worker config
- `FoundationaLLM:APIEndpoints:DataPipelineBackendWorker:*` - Backend worker config

**Worker Settings (`DataPipelineWorkerServiceSettings`):**
- `ParallelProcessorsCount`: Number of concurrent work item processors

## Development Guidelines

### Adding a New Stage Plugin

1. Create plugin class in `DataPipelinePlugins/Plugins/DataPipelineStage/`
2. Inherit from appropriate base class or implement `IDataPipelineStagePlugin`
3. Register plugin in `PluginNames.cs` and `PluginPackageManager.cs`
4. Define parameter names in `PluginParameterNames.cs`

### Adding a New Data Source Plugin

1. Create plugin class in `DataPipelinePlugins/Plugins/DataSource/`
2. Implement `IDataSourcePlugin` interface
3. Register in plugin package manager

### Testing Patterns

- Unit tests in `tests/dotnet/DataPipeline.Tests/`
- Integration tests should mock Cosmos DB and Storage services
- Use `NullDataPipelineServiceClient` for isolated testing

## File Structure Reference

```
src/dotnet/
├── DataPipelineAPI/                    # Main API service
│   ├── Controllers/
│   ├── Program.cs
│   └── Dockerfile
├── DataPipelineFrontendWorker/         # Frontend processing worker
│   ├── Program.cs
│   └── Dockerfile
├── DataPipelineBackendWorker/          # Backend processing worker
│   ├── Program.cs
│   └── Dockerfile
├── DataPipelineEngine/                 # Core engine library
│   ├── Services/
│   │   ├── DataPipelineService.cs
│   │   ├── DataPipelineTriggerService.cs
│   │   ├── DataPipelineRunnerService.cs
│   │   ├── DataPipelineWorkerService.cs
│   │   ├── DataPipelineStateService.cs
│   │   └── Runners/
│   │       ├── DataPipelineRunner.cs
│   │       └── DataPipelineStageRunner.cs
│   ├── Interfaces/
│   └── Models/
├── DataPipeline/                       # Resource provider library
│   ├── ResourceProviders/
│   │   └── DataPipelineResourceProviderService.cs
│   ├── Clients/
│   ├── Validation/
│   └── Interfaces/
├── DataPipelinePlugins/                # Plugin implementations
│   └── Plugins/
│       ├── DataSource/
│       ├── ContentTextExtraction/
│       ├── ContentTextPartitioning/
│       └── DataPipelineStage/
└── Common/
    ├── Models/
    │   ├── ResourceProviders/DataPipeline/
    │   └── DataPipelines/
    └── Constants/DataPipelines/
```
