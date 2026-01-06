# Knowledge Sources

This section describes how to configure knowledge sources in FoundationaLLM. Knowledge sources provide the contextual information that agents use to answer questions and perform tasks.

## Overview

FoundationaLLM supports multiple types of knowledge sources:

- **SharePoint Online** - Connect to SharePoint document libraries
- **Azure Data Lake Storage** - Use ADLS Gen2 containers and folders
- **Knowledge Graphs** - Leverage structured entity-relationship data
- **Private Storage** - User-specific knowledge for custom agents

## Context Engineering

Context engineering is the process of preparing your documents and data to be used effectively by AI agents. This includes:

1. **Text Extraction** - Extract content from various file formats
2. **Text Partitioning** - Split content into meaningful chunks
3. **Embedding Generation** - Create vector representations for semantic search
4. **Indexing** - Store processed content for efficient retrieval

Data pipelines automate this context engineering process.

## Getting Started

- [Context Engineering Concepts](vectorization-concepts.md) - Understand how content is processed
- [Configuring Context Engineering](vectorization-configuration.md) - Set up processing services

## Knowledge Source Guides

Configure various knowledge sources:

- [SharePoint Online Guide](sharepoint-upload-guide.md) - Use SharePoint document libraries or OneDrive as a knowledge source
- [Azure Data Lake Guide](azure-data-lake-guide.md) - Use Azure Data Lake Storage as a knowledge source
- [Knowledge Graph Source](knowledge-graph-source.md) - Use Knowledge Graphs as a knowledge source

## Configuration & Operations

- [Managing Profiles](vectorization-profiles.md) - Configure data sources, text partitioning, embeddings, and indexing
- [Triggering Processing](vectorization-triggering.md) - Run data pipelines manually or on schedule
- [Monitoring and Troubleshooting](vectorization-monitoring-troubleshooting.md) - Debug processing issues

## Data Pipelines

Data pipelines orchestrate the context engineering process:

- [Data Pipeline Management](../management-ui/data-pipeline-management.md) - Create and manage data pipelines via Management Portal
- [Data Pipeline Concepts](../../concepts/data-pipeline/data-pipeline.md) - Understand data pipeline architecture

## Terminology

| Term | Description |
|------|-------------|
| **Knowledge Source** | A repository of documents or data that provides context to agents |
| **Data Pipeline** | An automated workflow that processes knowledge source content |
| **Context Engineering** | The process of preparing content for AI agent consumption |
| **Embedding** | A vector representation of text for semantic search |
| **Indexing** | Storing processed content in a searchable format |
