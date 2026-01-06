# Knowledge Graph as a Knowledge Source

FoundationaLLM supports Knowledge Graphs as a knowledge source, enabling agents to leverage structured, relationship-rich data for more intelligent responses.

## Overview

A Knowledge Graph provides a structured representation of information through entities and their relationships. Unlike vector-based retrieval, knowledge graphs enable:

- **Relationship traversal**: Navigate connections between concepts
- **Structured queries**: Precise data retrieval
- **Contextual understanding**: Rich semantic relationships
- **Entity resolution**: Connect related information

## Use Cases

| Scenario | Benefit |
|----------|---------|
| Enterprise knowledge base | Navigate complex organizational data |
| Product catalogs | Traverse product relationships |
| Research databases | Connect scientific concepts |
| Compliance data | Follow regulatory relationships |

## Architecture

```
Documents          Data Pipeline         Knowledge Graph
    |                   |                      |
    |------------------>|                      |
    |                   | Extract entities     |
    |                   | Extract relationships|
    |                   |--------------------->|
    |                   |                      |
                                               |
Agent <----------------------------------------|
        Query via Graph Protocol               |
```

## Prerequisites

Before using Knowledge Graph as a knowledge source:

<!-- [TODO: Document specific prerequisites when implementation is confirmed] -->

1. Knowledge Graph backend configured
2. Data pipeline with entity/relationship extraction
3. Agent configured for graph queries

## Supported Graph Databases

<!-- [TODO: Confirm supported graph databases] -->

| Database | Status | Notes |
|----------|--------|-------|
| TBD | TBD | TBD |

## Setting Up Knowledge Graph

### Step 1: Configure Graph Backend

<!-- [TODO: Document graph backend configuration when available] -->

### Step 2: Create Data Pipeline with Graph Extraction

Configure a data pipeline that includes entity and relationship extraction:

```json
{
  "stages": [
    {
      "name": "Extract",
      "plugin_object_id": "...TextExtractionDataPipelineStage"
    },
    {
      "name": "EntityExtraction",
      "plugin_object_id": "...EntityExtractionDataPipelineStage",
      "plugin_parameters": [
        {
          "parameter_metadata": {
            "name": "EntityTypes",
            "type": "array"
          },
          "default_value": ["Person", "Organization", "Location", "Concept"]
        }
      ]
    },
    {
      "name": "RelationshipExtraction",
      "plugin_object_id": "...RelationshipExtractionDataPipelineStage"
    },
    {
      "name": "GraphAssembly",
      "plugin_object_id": "...KnowledgeGraphAssemblyDataPipelineStage"
    }
  ]
}
```

<!-- [TODO: Provide actual plugin identifiers when available] -->

### Step 3: Configure Agent to Use Knowledge Graph

Connect your agent to the knowledge graph:

<!-- [TODO: Document agent configuration for knowledge graph] -->

## Query Patterns

### Entity Lookup

Find information about a specific entity:

```
User: "Tell me about Project Alpha"
Agent: [Queries graph for 'Project Alpha' entity and its properties]
```

### Relationship Traversal

Navigate connections between entities:

```
User: "Who works on Project Alpha?"
Agent: [Traverses 'works_on' relationships from 'Project Alpha']
```

### Path Finding

Find connections between entities:

```
User: "How is John related to Project Alpha?"
Agent: [Finds paths between 'John' and 'Project Alpha']
```

## Configuration

### Knowledge Graph Service Configuration

<!-- [TODO: Document configuration settings] -->

| Setting | Description |
|---------|-------------|
| `FoundationaLLM:APIEndpoints:ContextAPI:Configuration:KnowledgeGraphService:Embedding` | Embedding configuration for graph queries |

### Embedding Configuration Structure

```json
{
  "model": "text-embedding-3-large",
  "dimensions": 2048,
  "endpoint": "...",
  "api_key": "..."
}
```

## Combining with Vector Search

Knowledge graphs can be used alongside vector search:

| Mode | Use Case |
|------|----------|
| Graph only | Structured, relationship-focused queries |
| Vector only | Semantic similarity search |
| Hybrid | Combine structured and semantic retrieval |

<!-- [TODO: Document hybrid search configuration] -->

## Performance Considerations

| Factor | Impact | Recommendation |
|--------|--------|----------------|
| Graph size | Query latency | Index frequently queried paths |
| Query complexity | Response time | Limit traversal depth |
| Concurrent queries | Throughput | Scale graph backend |

## Monitoring

### Graph Statistics

Monitor your knowledge graph health:

<!-- [TODO: Document monitoring capabilities] -->

| Metric | Description |
|--------|-------------|
| Entity count | Total entities in graph |
| Relationship count | Total relationships |
| Query latency | Average query time |

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| No results returned | Entity not in graph | Verify pipeline processed source |
| Slow queries | Complex traversals | Optimize query patterns |
| Missing relationships | Extraction gaps | Review extraction configuration |

## Current Status

<!-- [TODO: Update with current implementation status] -->

> [!NOTE]
> Knowledge Graph integration is actively being developed. Some features described here may be in preview or planned for future releases.

For the latest status on Knowledge Graph capabilities, refer to the release notes.

## Related Topics

- [Data Pipeline Concepts](../../concepts/data-pipeline/data-pipeline.md)
- [Data Pipeline Management](../management-ui/data-pipeline-management.md)
- [Azure Data Lake Guide](azure-data-lake-guide.md)
