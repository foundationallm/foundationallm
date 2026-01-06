# User Portal Guide

The FoundationaLLM User Portal is the primary interface for end-users to interact with AI agents. This section provides comprehensive documentation for using and customizing the User Portal experience.

## Overview

The User Portal provides the following capabilities:

- **Chat with Agents**: Interact with configured AI agents through a conversational interface
- **Agent Selection**: Choose from available agents in the agent dropdown
- **Agent Management**: Customize which agents appear in your workspace
- **File Upload**: Share files with agents for analysis and processing
- **Conversation History**: Access and continue previous conversations

## Getting Started

- [Quickstart Guide](../quickstart.md) - Get started with your first conversation

## Agent Management

Learn how to manage and customize your agent experience:

- [Managing Your Agent Catalog](agent-management.md) - Control which agents appear in your workspace
- [Self-Service Agent Creation](self-service-agent-creation.md) - Create and customize your own agents
- [Agent Sharing Model](agent-sharing-model.md) - Understand owner, collaborator, and user roles

## Accessibility

- [Accessibility Features](accessibility.md) - WCAG compliance and accessibility support

## UX Walkthroughs

Step-by-step guides with screenshots:

- [Agent Management Walkthrough](walkthroughs/agent-management-walkthrough.md)
- [Agent Creation Walkthrough](walkthroughs/agent-creation-walkthrough.md)
- [Status Message Walkthrough](walkthroughs/status-message-walkthrough.md)

## User Portal vs. Management Portal

Understanding the difference between the two portals:

| Feature | User Portal | Management Portal |
|---------|-------------|-------------------|
| Chat with agents | ✅ | ❌ |
| View available agents | ✅ | ✅ |
| Select personal agent catalog | ✅ | ❌ |
| Create self-service agents | ✅ | ✅ |
| Configure platform agents | ❌ | ✅ |
| Manage vectorization | ❌ | ✅ |
| Manage data pipelines | ❌ | ✅ |
| Configure branding | ❌ | ✅ |
| Publish system status messages | ❌ | ✅ |

> [!NOTE]
> Self-service agent creation in the User Portal requires the `FoundationaLLM.Agent.SelfService` feature flag to be enabled (default: `true`).

## Configuration

The User Portal behavior can be customized through the following App Configuration settings:

| Setting | Description |
|---------|-------------|
| `FoundationaLLM:UserPortal:Configuration:FeaturedAgentNames` | Comma-separated list of featured agent names |
| `FoundationaLLM:UserPortal:Configuration:ShowFileUpload` | Enable/disable file upload capability |
| `FoundationaLLM:UserPortal:Configuration:ShowMessageRating` | Enable/disable message rating |
| `FoundationaLLM:UserPortal:Configuration:ShowMessageTokens` | Enable/disable token count display |
| `FoundationaLLM:UserPortal:Configuration:ShowViewPrompt` | Enable/disable "View Prompt" button |
| `FoundationaLLM:UserPortal:Configuration:ShowLastConversationOnStartup` | Show last conversation on login |

For more configuration options, see [App Configuration Values](../../deployment/app-configuration-values.md).

## Agent Capabilities

Agents in FoundationaLLM support a variety of capabilities that enhance user interactions:

| Capability | Description |
|------------|-------------|
| **Conversation History** | Maintains context across exchanges in a session |
| **Content Safety** | Filters harmful content using Azure Content Safety |
| **Data Protection** | Masks PII using Microsoft Presidio |
| **Semantic Cache** | Reduces latency by caching similar queries |
| **Prompt Rewriting** | Improves prompt quality automatically |

For detailed documentation on agent capabilities, see [Agents and Workflows](../agents/agents_workflows.md#agent-configuration-section).
