# Data Pipeline Concepts

This document provides visual representations of the core concepts related to data pipelines, knowledge sources, and knowledge units in FoundationaLLM.

## Knowledge Source and Knowledge Unit Relationship

A Knowledge Source represents a collection of related data, while Knowledge Units are the actual indexed data that can be queried. Each Knowledge Source can have multiple Knowledge Units associated with it.

```mermaid
graph LR
    KS[Knowledge Source<br/>Contains metadata and<br/>references to knowledge units]
    KU1[Knowledge Unit 1<br/>Points to vector database<br/>and optional knowledge graph]
    KU2[Knowledge Unit 2<br/>Points to vector database<br/>and optional knowledge graph]
    KU3[Knowledge Unit 3<br/>Points to vector database<br/>and optional knowledge graph]
    
    VDB1[(Vector Database 1<br/>Stores embeddings)]
    VDB2[(Vector Database 2<br/>Stores embeddings)]
    VDB3[(Vector Database 3<br/>Stores embeddings)]
    
    KG1[(Knowledge Graph<br/>Vector Database 1)]
    KG3[(Knowledge Graph<br/>Vector Database 3)]
    
    KS -->|knowledge_unit_object_ids| KU1
    KS -->|knowledge_unit_object_ids| KU2
    KS -->|knowledge_unit_object_ids| KU3
    
    KU1 -->|vector_database_object_id| VDB1
    KU2 -->|vector_database_object_id| VDB2
    KU3 -->|vector_database_object_id| VDB3
    
    KU1 -->|knowledge_graph_vector_database_object_id| KG1
    KU3 -->|knowledge_graph_vector_database_object_id| KG3
    
    style KS fill:#e1f5ff
    style KU1 fill:#fff4e1
    style KU2 fill:#fff4e1
    style KU3 fill:#fff4e1
    style VDB1 fill:#e8f5e9
    style VDB2 fill:#e8f5e9
    style VDB3 fill:#e8f5e9
    style KG1 fill:#f3e5f5
    style KG3 fill:#f3e5f5
```

**Key Points:**
- Each Knowledge Source contains a list of `knowledge_unit_object_ids` referencing its Knowledge Units
- Each Knowledge Unit has a required `vector_database_object_id` pointing to a vector database for semantic search
- Each Knowledge Unit can optionally have a `knowledge_graph_vector_database_object_id` for graph-based queries
- Knowledge Units also have an optional `vector_store_id` within the vector database

## Data Pipeline Structure

A Data Pipeline consists of three main components: Data Source, Stages (structured as a forest), and Triggers.

```mermaid
graph TD
    DP[Data Pipeline Definition<br/>Configuration and metadata]
    
    DS[Data Pipeline Data Source<br/>Plugin-based data ingestion]
    DSR[(Data Source Resource<br/>e.g., Azure Data Lake,<br/>SharePoint Online)]
    DSP[Data Source Plugin<br/>Implements data reading]
    
    SS1[Starting Stage 1<br/>e.g., Text Extraction]
    SS2[Starting Stage 2<br/>e.g., Image Processing]
    
    S1[Stage: Partition<br/>Split text into chunks]
    S2[Stage: Embed<br/>Generate embeddings]
    S3[Stage: Index<br/>Store in vector DB]
    
    S4[Stage: Alternative Path<br/>Different processing]
    
    P1[Stage Plugin<br/>Implements logic]
    P2[Dependency Plugin<br/>Helper functionality]
    
    T1[Trigger: Schedule<br/>Cron-based execution]
    T2[Trigger: Event<br/>File change detection]
    T3[Trigger: Manual<br/>User-initiated]
    
    DP --> DS
    DP --> SS1
    DP --> SS2
    DP --> T1
    DP --> T2
    DP --> T3
    
    DS -->|data_source_object_id| DSR
    DS -->|plugin_object_id| DSP
    
    SS1 -->|next_stages| S1
    SS2 -->|next_stages| S4
    S1 -->|next_stages| S2
    S2 -->|next_stages| S3
    
    S1 -->|plugin_object_id| P1
    S1 -->|plugin_dependencies| P2
    
    T1 -.->|provides parameter_values| DS
    T1 -.->|provides parameter_values| S1
    T1 -.->|provides parameter_values| S2
    T1 -.->|provides parameter_values| S3
    
    style DP fill:#e1f5ff
    style DS fill:#fff4e1
    style DSR fill:#e8f5e9
    style DSP fill:#f3e5f5
    style SS1 fill:#fff4e1
    style SS2 fill:#fff4e1
    style S1 fill:#fff4e1
    style S2 fill:#fff4e1
    style S3 fill:#fff4e1
    style S4 fill:#fff4e1
    style P1 fill:#f3e5f5
    style P2 fill:#f3e5f5
    style T1 fill:#ffebee
    style T2 fill:#ffebee
    style T3 fill:#ffebee
```

**Key Points:**
- **Data Source**: References a FoundationaLLM DataSource resource and uses a plugin to read data
- **Stages**: Organized as a forest (multiple starting points), each stage has a plugin and optional dependencies
- **Triggers**: Schedule, Event, or Manual triggers that provide parameter values to execute the pipeline
- **Plugins**: Both data sources and stages use plugins for implementation, with configurable parameters

## Data Pipeline Execution Flow

This diagram shows the lifecycle of a data pipeline execution from trigger to completion.

```mermaid
sequenceDiagram
    participant T as Trigger<br/>(Schedule/Event/Manual)
    participant API as DataPipelineAPI
    participant Engine as DataPipelineEngine
    participant Runner as DataPipelineRunner
    participant State as DataPipelineStateService
    participant Queue as Azure Storage Queue
    participant Worker as DataPipelineWorker
    participant Stage as DataPipelineStageRunner
    participant Plugin as Stage Plugin
    participant VDB as Vector Database
    
    T->>API: Trigger pipeline execution<br/>(with parameter values)
    API->>Engine: Create DataPipelineRun
    Engine->>State: Initialize run state
    State-->>Engine: State initialized
    
    Engine->>Runner: InitializeNew(run, contentItems)
    Runner->>State: InitializeDataPipelineRunState
    State-->>Runner: State created
    
    loop For each starting stage
        Runner->>Stage: CreateStageRunner(stage, workItems)
        Stage->>Queue: Enqueue work items
    end
    
    Runner-->>Engine: Pipeline run initialized
    Engine-->>API: Run ID returned
    
    loop Processing work items
        Worker->>Queue: Dequeue work item
        Queue-->>Worker: Work item message
        
        Worker->>Stage: Process work item
        Stage->>Plugin: Execute plugin logic
        Plugin->>Plugin: Transform content<br/>(extract/partition/embed/index)
        
        alt If indexing stage
            Plugin->>VDB: Store embeddings
            VDB-->>Plugin: Stored successfully
        end
        
        Plugin-->>Stage: Processing complete
        Stage->>State: Update content item state
        
        alt If stage has next stages
            Stage->>Queue: Enqueue items for next stage
        end
        
        Stage-->>Worker: Work item complete
    end
    
    Worker->>State: Check pipeline completion
    State-->>Worker: All stages complete
    Worker->>Engine: Mark run as completed
```

**Key Points:**
- Triggers initiate pipeline runs with parameter values
- Engine creates a DataPipelineRun and initializes state
- Runner creates stage runners for starting stages
- Work items are enqueued to Azure Storage Queues
- Workers dequeue and process items through stage plugins
- State service tracks progress of content items through stages
- Upon completion, items may be enqueued for subsequent stages
- Pipeline completes when all stages have processed all items

## Data Pipeline and Knowledge Unit Integration

This diagram shows how data pipelines populate knowledge units that are referenced by knowledge sources.

```mermaid
graph TB
    subgraph "Data Sources"
        ADL[(Azure Data Lake<br/>Raw documents)]
        SPO[(SharePoint Online<br/>Documents and files)]
        PS[(Private Storage<br/>Custom data)]
    end
    
    subgraph "Data Pipeline Execution"
        DP[Data Pipeline Definition<br/>Extract → Partition → Embed → Index]
        
        DS[Data Source Stage<br/>Read files from storage]
        EX[Extraction Stage<br/>Extract text from PDFs, DOCX, etc.]
        PA[Partition Stage<br/>Split into chunks]
        EM[Embedding Stage<br/>Generate vector embeddings]
        IX[Indexing Stage<br/>Store in vector database]
        
        KGE[Knowledge Graph Stages<br/>Optional: Extract entities,<br/>relationships, consolidate]
        
        DP --> DS
        DS --> EX
        EX --> PA
        PA --> EM
        EM --> IX
        EM --> KGE
    end
    
    subgraph "Knowledge Storage"
        VDB[(Vector Database<br/>Azure AI Search<br/>Stores embeddings<br/>and metadata)]
        
        KGVDB[(Knowledge Graph<br/>Vector Database<br/>Stores graph embeddings)]
    end
    
    subgraph "Knowledge Organization"
        KU[Knowledge Unit<br/>Points to vector DB<br/>and optional KG DB]
        KS[Knowledge Source<br/>Collection of<br/>knowledge units]
    end
    
    subgraph "Agent Access"
        Agent[FoundationaLLM Agent<br/>Uses knowledge source<br/>for RAG]
        User[User Query]
    end
    
    ADL -->|files| DS
    SPO -->|files| DS
    PS -->|files| DS
    
    IX -->|writes embeddings| VDB
    KGE -->|writes graph data| KGVDB
    
    VDB -.->|referenced by<br/>vector_database_object_id| KU
    KGVDB -.->|referenced by<br/>knowledge_graph_vector_database_object_id| KU
    
    KU -->|knowledge_unit_object_ids| KS
    
    KS -->|configured in agent| Agent
    User -->|queries| Agent
    Agent -->|searches| VDB
    Agent -->|queries| KGVDB
    
    style ADL fill:#e8f5e9
    style SPO fill:#e8f5e9
    style PS fill:#e8f5e9
    style DP fill:#e1f5ff
    style DS fill:#fff4e1
    style EX fill:#fff4e1
    style PA fill:#fff4e1
    style EM fill:#fff4e1
    style IX fill:#fff4e1
    style KGE fill:#fff4e1
    style VDB fill:#e8f5e9
    style KGVDB fill:#f3e5f5
    style KU fill:#fff4e1
    style KS fill:#e1f5ff
    style Agent fill:#ffebee
    style User fill:#f5f5f5
```

**Key Points:**
- Data pipelines read from various data sources (Azure Data Lake, SharePoint, etc.)
- Pipeline stages transform data: Extract → Partition → Embed → Index
- Optional knowledge graph stages can extract entities and relationships
- Embeddings are stored in vector databases (e.g., Azure AI Search)
- Knowledge graph data is stored in separate graph vector databases
- Knowledge Units reference the vector databases where data is stored
- Knowledge Sources aggregate multiple Knowledge Units
- Agents use Knowledge Sources for Retrieval-Augmented Generation (RAG)
- Users query agents, which in turn search the vector databases

## Complete End-to-End Flow

This diagram provides a comprehensive view of how data flows from source systems to user queries.

```mermaid
flowchart TD
    Start([Raw Data in Storage])
    
    subgraph "Phase 1: Pipeline Definition"
        DefineDP[Define Data Pipeline<br/>- Configure data source<br/>- Define stages<br/>- Set up triggers]
        DefineDS[Configure Data Source<br/>- Azure Data Lake<br/>- SharePoint Online<br/>- Private Storage]
        DefineKU[Create Knowledge Unit<br/>- Set vector DB reference<br/>- Optional: KG DB reference]
        DefineKS[Create Knowledge Source<br/>- Add knowledge unit refs]
    end
    
    subgraph "Phase 2: Pipeline Execution"
        Trigger[Trigger Fires<br/>Schedule/Event/Manual]
        Execute[Execute Pipeline<br/>Process content items]
        
        Stage1[Stage 1: Extract Text<br/>PDF, DOCX, XLSX, Images]
        Stage2[Stage 2: Partition<br/>Token or Semantic chunking]
        Stage3[Stage 3: Embed<br/>Generate vector embeddings]
        Stage4[Stage 4: Index<br/>Store in vector database]
        
        KGStage[Optional KG Stages<br/>Extract, Consolidate,<br/>Summarize, Embed, Index]
    end
    
    subgraph "Phase 3: Data Storage"
        VectorDB[(Vector Database<br/>Searchable embeddings)]
        GraphDB[(Knowledge Graph DB<br/>Entity relationships)]
    end
    
    subgraph "Phase 4: Agent Configuration"
        ConfigAgent[Configure Agent<br/>with Knowledge Source]
        Agent[Agent Ready<br/>for User Queries]
    end
    
    subgraph "Phase 5: User Interaction"
        UserQuery[User Submits Query]
        Search[Agent Searches<br/>Vector DB & KG]
        Retrieve[Retrieve Relevant<br/>Documents & Entities]
        Generate[Generate Response<br/>with LLM]
        Response([Return Answer to User])
    end
    
    Start --> DefineDP
    DefineDP --> DefineDS
    DefineDS --> DefineKU
    DefineKU --> DefineKS
    
    DefineKS --> Trigger
    Trigger --> Execute
    Execute --> Stage1
    Stage1 --> Stage2
    Stage2 --> Stage3
    Stage3 --> Stage4
    Stage3 --> KGStage
    
    Stage4 --> VectorDB
    KGStage --> GraphDB
    
    VectorDB -.->|referenced by| DefineKU
    GraphDB -.->|referenced by| DefineKU
    
    DefineKS --> ConfigAgent
    ConfigAgent --> Agent
    
    Agent --> UserQuery
    UserQuery --> Search
    Search --> Retrieve
    Retrieve --> Generate
    Generate --> Response
    
    style Start fill:#e8f5e9
    style DefineDP fill:#e1f5ff
    style DefineDS fill:#e1f5ff
    style DefineKU fill:#fff4e1
    style DefineKS fill:#fff4e1
    style Trigger fill:#ffebee
    style Execute fill:#ffebee
    style Stage1 fill:#fff4e1
    style Stage2 fill:#fff4e1
    style Stage3 fill:#fff4e1
    style Stage4 fill:#fff4e1
    style KGStage fill:#f3e5f5
    style VectorDB fill:#e8f5e9
    style GraphDB fill:#f3e5f5
    style ConfigAgent fill:#e1f5ff
    style Agent fill:#ffebee
    style UserQuery fill:#f5f5f5
    style Search fill:#fff4e1
    style Retrieve fill:#fff4e1
    style Generate fill:#fff4e1
    style Response fill:#e8f5e9
```

**Complete Flow Summary:**

1. **Phase 1: Setup & Definition**
   - Define data pipeline with stages and triggers
   - Configure data source connections
   - Create knowledge units pointing to target databases
   - Create knowledge sources aggregating knowledge units

2. **Phase 2: Data Processing**
   - Pipeline trigger fires (schedule, event, or manual)
   - Content items flow through stages: Extract → Partition → Embed → Index
   - Optional knowledge graph stages extract entities and relationships
   - Each stage transforms data and passes to next stage

3. **Phase 3: Persistent Storage**
   - Vector embeddings stored in vector databases (Azure AI Search)
   - Knowledge graph data stored in graph databases
   - Knowledge units reference these storage locations

4. **Phase 4: Agent Setup**
   - Agents configured with knowledge sources
   - Knowledge sources provide access to all associated knowledge units
   - Agent ready to serve user queries

5. **Phase 5: Query & Response**
   - User submits natural language query
   - Agent searches vector databases and knowledge graphs
   - Relevant documents and entities retrieved
   - LLM generates contextual response using retrieved information
   - Response returned to user

## Key Concepts Summary

| Concept | Description | Purpose |
|---------|-------------|---------|
| **Knowledge Source** | Collection of knowledge units | Aggregates multiple knowledge units for agent use |
| **Knowledge Unit** | References to indexed data | Points to vector DB and optional knowledge graph |
| **Data Pipeline** | Automated data processing workflow | Transforms raw data into searchable knowledge |
| **Data Source** | Source of raw data | Provides input files/documents for processing |
| **Pipeline Stage** | Single processing step | Performs specific transformation (extract, partition, embed, index) |
| **Pipeline Trigger** | Execution initiator | Starts pipeline on schedule, event, or manual action |
| **Plugin** | Pluggable implementation | Provides specific functionality for data sources and stages |
| **Vector Database** | Embedding storage | Enables semantic search over processed data |
| **Knowledge Graph** | Entity relationship storage | Enables graph-based queries and reasoning |

## Parameter Naming Convention

When triggers provide parameter values to execute pipelines, they follow a specific naming convention:

- **Data Source Parameters**: `DataSource.{DataSourceName}.{ParameterName}`
  - Example: `DataSource.VGDataLake.Folders`

- **Stage Parameters**: `Stage.{StageName}.{ParameterName}`
  - Example: `Stage.Partition.PartitioningStrategy`

- **Stage Dependency Parameters**: `Stage.{StageName}.Dependency.{DependencyPluginName}.{ParameterName}`
  - Example: `Stage.Partition.Dependency.TokenContentTextPartitioning.PartitionSizeTokens`

This convention allows triggers to provide all necessary configuration values for pipeline execution without user interaction.

## Related Documentation

- [Data Pipelines Reference](../docs/management-portal/reference/concepts/data-pipelines.md)
- [Creating Data Pipelines](../docs/management-portal/how-to-guides/data/data-pipelines/creating-data-pipelines.md)
- [Knowledge Graph Integration](../docs/management-portal/how-to-guides/data/knowledge-sources/knowledge-graph-integration.md)
- [Plugin Documentation](../docs/concepts/plugin/plugin.md)
