# Knowledge Graph Integration

Learn how to use knowledge graphs as a knowledge source for FoundationaLLM agents.

## Overview

Knowledge graphs provide structured, relationship-rich data that enhances agent capabilities:

- **Semantic Relationships**: Understand connections between entities
- **Contextual Answers**: Provide richer, more accurate responses
- **Graph Traversal**: Follow relationships to find related information

## Supported Graph Formats

| Format | Description |
|--------|-------------|
| **Neo4j** | Popular graph database |
| **Azure Cosmos DB (Gremlin API)** | Azure-native graph database |
| **RDF/SPARQL** | W3C standard semantic web format |
| **Custom JSON-LD** | Linked data format |

## Use Cases

| Scenario | Benefits |
|----------|----------|
| **Organizational Data** | Employee relationships, reporting structures |
| **Product Catalogs** | Product relationships, accessories, alternatives |
| **Research** | Citation networks, topic relationships |
| **Customer Data** | Customer relationships, interaction history |

## Configuration Overview

### Prerequisites

- A knowledge graph database populated with your data
- Network connectivity from FoundationaLLM to the graph database
- Authentication credentials configured

### Integration Steps

1. **Prepare Your Knowledge Graph** - Ensure data follows expected schema
2. **Create a Data Source** - Configure connection to the graph database
3. **Create a Data Pipeline** - Process graph data for agent use
4. **Configure Agent** - Associate the knowledge source with your agent

## Data Source Configuration

> **TODO**: Document specific data source configuration steps for knowledge graph types when available in the UI, including connection parameters, authentication, and query configuration.

### Connection Settings

| Setting | Description |
|---------|-------------|
| **Endpoint** | Graph database endpoint URL |
| **Authentication** | Credentials or managed identity |
| **Database/Graph Name** | Specific database or graph to query |

## Schema Requirements

### Node Structure

Nodes should include identifiable properties:

```json
{
  "id": "unique-node-identifier",
  "type": "Person",
  "properties": {
    "name": "John Smith",
    "title": "Software Engineer",
    "department": "Engineering",
    "description": "Senior developer specializing in AI systems"
  }
}
```

**Required Properties:**

| Property | Description |
|----------|-------------|
| `id` | Unique identifier for the node |
| `type` | Entity type/label |
| `name` | Human-readable name |
| `description` | Text description for search/context |

### Relationship Structure

Relationships connect nodes with semantic meaning:

```json
{
  "source": "person-123",
  "target": "person-456",
  "type": "REPORTS_TO",
  "properties": {
    "since": "2023-01-15"
  }
}
```

**Relationship Components:**

| Component | Description |
|-----------|-------------|
| `source` | Starting node ID |
| `target` | Ending node ID |
| `type` | Relationship label (e.g., REPORTS_TO, KNOWS, CONTAINS) |
| `properties` | Optional metadata about the relationship |

## Data Pipeline Configuration

### Graph Processing Stages

> **TODO**: Document specific pipeline stages for processing knowledge graphs.

Common stages include:

1. **Graph Query** - Execute queries to extract relevant data
2. **Entity Extraction** - Convert nodes to searchable documents
3. **Embedding** - Generate vector embeddings for semantic search
4. **Indexing** - Store in vector database for retrieval

### Query Patterns

**Cypher (Neo4j) Example:**
```cypher
MATCH (p:Person)-[r:WORKS_IN]->(d:Department)
RETURN p.name, p.description, d.name AS department
```

**Gremlin (Cosmos DB) Example:**
```groovy
g.V().hasLabel('Person').out('works_in').hasLabel('Department')
```

## Best Practices

### Schema Design

| Practice | Description |
|----------|-------------|
| **Consistent Types** | Use standard entity type names |
| **Meaningful Relationships** | Clear, semantic relationship labels |
| **Rich Properties** | Include descriptive text for search |
| **Avoid Deep Nesting** | Flatten complex structures when possible |

### Query Optimization

- Limit result set sizes
- Use indexes on frequently queried properties
- Consider materialized views for complex queries

### Content Quality

- Include descriptive text in node properties
- Keep descriptions concise but informative
- Update graph data regularly

## Integration Patterns

### Direct Query

Agents query the graph database directly for specific relationships.

### Vector Search

Graph data is processed into vector embeddings for semantic search.

### Hybrid Approach

Combine vector search with graph traversal for comprehensive answers.

## Troubleshooting

### Connection Issues

- Verify endpoint URL is correct
- Check authentication credentials
- Review network/firewall configuration

### Query Failures

- Validate query syntax for your graph database
- Check for schema mismatches
- Review timeout settings for large queries

### Poor Results

- Ensure node descriptions are meaningful
- Check that relevant data is being indexed
- Review embedding quality

## Limitations

| Limitation | Description |
|------------|-------------|
| **Graph Size** | Very large graphs may require query optimization |
| **Real-time Sync** | Graph changes may not immediately reflect |
| **Complex Queries** | Some multi-hop queries may be slow |

## Related Topics

- [Data Pipelines](../data-pipelines/creating-data-pipelines.md)
- [Data Sources](../data-sources.md)
- [Azure Data Lake Knowledge Source](azure-data-lake.md)
