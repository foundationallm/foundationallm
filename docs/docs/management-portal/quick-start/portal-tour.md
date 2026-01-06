# Tour of the Management Portal

A guided tour of the FoundationaLLM Management Portal interface and its key features.

## Overview

The Management Portal is the administrative interface for configuring and managing your FoundationaLLM deployment. It provides IT administrators, developers, and power users with tools to create agents, configure data pipelines, manage security, and customize the platform.

## Accessing the Management Portal

1. Navigate to your Management Portal URL (typically `https://<your-instance>-portal.azurewebsites.net`)
2. Sign in with your Microsoft Entra ID credentials
3. Upon successful authentication, you'll see the main dashboard

## Main Navigation

The sidebar on the left side of the screen provides access to all portal sections. The sidebar can be collapsed by clicking the arrow button in the header.

### Agents Section

| Menu Item | Description |
|-----------|-------------|
| **Create New Agent** | Step-by-step wizard for creating and configuring new AI agents |
| **All Agents** | View and manage all agents in the instance |
| **My Agents** | View agents you've created or own |
| **Prompts** | Manage prompt templates used by agents |

### Data Section

| Menu Item | Description |
|-----------|-------------|
| **Data Sources** | Configure connections to data repositories (Azure Data Lake, SharePoint, etc.) |
| **Data Pipelines** | Create and manage data processing pipelines |
| **Data Pipeline Runs** | Monitor pipeline execution and view run history |
| **Knowledge Sources** | Configure knowledge bases for agent retrieval |

### Models and Endpoints Section

| Menu Item | Description |
|-----------|-------------|
| **AI Models** | Configure AI model deployments (Azure OpenAI, etc.) |
| **API Endpoints** | Manage API endpoint configurations |

### Security Section

| Menu Item | Description |
|-----------|-------------|
| **Instance Access Control** | Manage role assignments and access permissions |

### FLLM Platform Section

| Menu Item | Description |
|-----------|-------------|
| **Branding** | Customize the portal appearance (colors, logos, text) |
| **Configuration** | Configure portal access and instance-level settings |
| **Deployment Information** | View instance ID, API status, and deployment details |

## Common UI Elements

### Page Headers

Each page includes:
- **Page title**: Identifies the current section
- **Subheader**: Brief description of the page's purpose
- **Action buttons**: Context-specific actions (Create, Refresh, etc.)

### Data Tables

Resource lists use consistent table layouts:
- **Sortable columns**: Click column headers to sort
- **Search**: Filter results by name or other fields
- **Pagination**: Navigate through large datasets (10, 25, 50, or 100 rows per page)
- **Edit/Delete actions**: Row-level actions for managing resources

### Forms and Wizards

Configuration pages use:
- **Step items**: Expandable cards that show current settings and allow editing
- **Toggle buttons**: Yes/No switches for enabling features
- **Dropdowns**: Selection lists for choosing options
- **Input validation**: Real-time feedback on required fields and format requirements

### Access Control

Resources with access control show:
- **Access Control button**: Opens role assignment dialog
- **Scope selection**: Choose the level at which permissions apply
- **Principal selection**: Add users, groups, or service principals

## User Account

The bottom of the sidebar displays:
- **User avatar**: Your profile image
- **Username**: Your display name (hover for full email)
- **Sign Out button**: Log out of the portal

## Keyboard Navigation

The portal supports keyboard navigation:
- `Tab` / `Shift+Tab`: Navigate between elements
- `Enter` / `Space`: Activate buttons and links
- `Escape`: Close dialogs and cancel operations

## Getting Started Workflow

For first-time users, the typical workflow is:

1. **Configure AI Models**: Set up your AI model deployments under Models and Endpoints
2. **Create Data Sources**: Connect to your data repositories under Data
3. **Create Prompts**: Define prompt templates under Agents > Prompts
4. **Create an Agent**: Use the Create New Agent wizard
5. **Configure Access**: Set up role assignments under Security
6. **Test**: Access the Chat User Portal to test your agent

## Related Topics

- [Creating Your First Agent](creating-first-agent.md)
- [Instance Access Control](../how-to-guides/security/instance-access-control.md)
- [Branding Configuration](../how-to-guides/fllm-platform/branding.md)
