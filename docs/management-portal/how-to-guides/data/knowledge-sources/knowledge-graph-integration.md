# Knowledge Graph Integration

Learn how to use knowledge graphs as a knowledge source for agents.

## Overview

Knowledge graphs provide structured, relationship-rich data that can enhance agent responses.

## Using a Knowledge Graph as a Knowledge Source

### Supported Formats
- Graph databases (Neo4j, Azure Cosmos DB Gremlin)
- RDF/SPARQL endpoints
- Custom graph formats

### Configuration Steps

1. Set up your knowledge graph
2. Configure a data source connection
3. Create a data pipeline with graph processing
4. Associate with an agent

## Required Schema/Format Expectations

### Node Structure
```json
{
  "id": "unique-identifier",
  "type": "entity-type",
  "properties": {
    "name": "Entity Name",
    "description": "Entity description"
  }
}
```

### Relationship Structure
```json
{
  "source": "node-id-1",
  "target": "node-id-2",
  "type": "RELATIONSHIP_TYPE",
  "properties": {}
}
```

## Best Practices

- Maintain consistent entity types
- Define clear relationship semantics
- Include descriptive properties

## Related Topics

- [Data Pipelines](../data-pipelines/creating-data-pipelines.md)
- [Data Sources](../data-sources.md)
