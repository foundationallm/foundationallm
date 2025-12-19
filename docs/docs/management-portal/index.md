# Management Portal

The Management Portal is the administrative interface for configuring and managing your FoundationaLLM deployment.

## Overview

The Management Portal provides IT administrators, developers, and power users with a comprehensive set of tools to:

- **Create and manage AI agents** - Configure agent behavior, workflows, and capabilities
- **Connect data sources** - Integrate with Azure Data Lake, SharePoint, and other repositories
- **Build data pipelines** - Process and index data for agent retrieval
- **Configure AI models** - Manage model deployments and API endpoints
- **Control access** - Implement role-based access control (RBAC)
- **Customize branding** - Personalize the portal appearance

## Target Audience

The Management Portal is designed for:

| Role | Primary Tasks |
|------|---------------|
| **IT Administrators** | Instance configuration, security settings, deployment management |
| **Developers** | Agent creation, workflow configuration, API integration |
| **Power Users** | Agent management, prompt engineering, data source configuration |

## Getting Started

### Quick Start

1. [Tour of the Management Portal](quick-start/portal-tour.md) - Learn the interface
2. [Creating Your First Agent](quick-start/creating-first-agent.md) - Build your first AI agent

### Prerequisites

- Microsoft Entra ID account with appropriate permissions
- Access to a deployed FoundationaLLM instance
- Reader or higher role assignment for Management Portal access

## Key Features

### Agent Management

Create and configure AI agents with:
- Multiple workflow types (OpenAI Assistants, LangGraph, Custom)
- Tool integration (Code Interpreter, DALL-E, Knowledge Search)
- Customizable system prompts
- Gatekeeper content safety

**Learn more:**
- [Create New Agent](how-to-guides/agents/create-new-agent.md)
- [All Agents](how-to-guides/agents/all-agents.md)
- [Prompts](how-to-guides/agents/prompts.md)

### Data Management

Connect and process organizational data:
- Configure data source connections
- Build data processing pipelines
- Monitor pipeline execution

**Learn more:**
- [Data Sources](how-to-guides/data/data-sources.md)
- [Creating Data Pipelines](how-to-guides/data/data-pipelines/creating-data-pipelines.md)
- [Knowledge Sources](how-to-guides/data/knowledge-sources/azure-data-lake.md)

### Security and Access Control

Implement comprehensive security:
- Role-based access control (RBAC)
- Agent-level permissions
- Agent access tokens for API integration

**Learn more:**
- [Instance Access Control](how-to-guides/security/instance-access-control.md)
- [Permissions and Roles Reference](reference/permissions-roles.md)

### Platform Configuration

Customize your deployment:
- Branding and visual identity
- Portal access configuration
- Deployment monitoring

**Learn more:**
- [Branding Configuration](how-to-guides/fllm-platform/branding.md)
- [Configuration](how-to-guides/fllm-platform/configuration.md)
- [Deployment Information](how-to-guides/fllm-platform/deployment-information.md)

## Portal Sections

| Section | Description |
|---------|-------------|
| **Agents** | Create, view, and manage AI agents and prompts |
| **Data** | Configure data sources, pipelines, and knowledge sources |
| **Models and Endpoints** | Manage AI model deployments and API endpoints |
| **Security** | Configure role assignments and access control |
| **FLLM Platform** | Branding, configuration, and deployment information |

## Reference Documentation

- [Agents & Workflows](reference/concepts/agents-workflows.md)
- [Data Pipelines](reference/concepts/data-pipelines.md)
- [Resource Management](reference/concepts/resource-management.md)
- [Configuration Reference](reference/configuration-reference.md)

## Related Topics

- [Chat User Portal Documentation](../chat-user-portal/index.md) - End-user chat interface
- [APIs & SDKs](../apis-sdks/index.md) - Programmatic access
- [Platform Operations](../platform-operations/index.md) - Deployment and operations
