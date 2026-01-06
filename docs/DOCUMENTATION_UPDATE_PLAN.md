# FoundationaLLM Documentation Update Plan

This document outlines the planned documentation updates for end-users, administrators, and developers using the FoundationaLLM APIs.

> **Status**: Initial documentation complete. See [DOCUMENTATION_UPDATE_SUMMARY.md](DOCUMENTATION_UPDATE_SUMMARY.md) for details.

## Terminology Standards

The following terminology standards have been applied throughout the documentation:

| Term | Usage | Notes |
|------|-------|-------|
| **Knowledge Source** | General term for document repositories | Replaces "vectorization" in most contexts |
| **Context Engineering** | Process of preparing content for AI agents | Includes extraction, partitioning, embedding |
| **Data Pipeline** | Automated workflow for processing content | Replaces "Vectorization Pipeline" |
| **Vectorization** | Only used when specifically referring to creating vector embeddings | Technical term, use sparingly |

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Documentation Structure Overview](#documentation-structure-overview)
3. [Detailed Topic Plans](#detailed-topic-plans)
   - [Knowledge Sources](#knowledge-sources)
   - [Data Pipeline](#data-pipeline)
   - [Image Description](#image-description)
   - [Knowledge Graph Integration](#knowledge-graph-integration)
   - [End-User Agent Management UX](#end-user-agent-management-ux)
   - [Self-Service Agent Creation UX](#self-service-agent-creation-ux)
   - [System Status Message](#system-status-message)
   - [UX Walk-throughs](#ux-walk-throughs)
   - [Core API / Platform](#core-api--platform)
   - [Accessibility Requirements](#accessibility-requirements)
4. [TODO Items Summary](#todo-items-summary)
5. [Implementation Priority](#implementation-priority)

---

## Executive Summary

This plan addresses 30 documentation topics across multiple areas. The documentation will be integrated into the existing structure without reorganizing the current hierarchy. Key areas include:

- **Knowledge Sources**: SharePoint Online, Azure Data Lake, private storage, and data pipeline documentation
- **User Portal Features**: Agent management, self-service agent creation, and status messages
- **API/Platform**: Quotas, rate limits, token limits, and OpenAI endpoint facades
- **Accessibility**: WCAG compliance documentation

### Existing Documentation Review

| Area | Existing Coverage | Gap Analysis |
|------|-------------------|--------------|
| SharePoint Online | Partial (vectorization profiles) | Needs end-user upload workflow |
| Azure Data Lake | Partial (vectorization profiles) | Needs knowledge source guide |
| Data Pipelines | Good conceptual docs | Needs monitoring/status UX guide |
| Agent Management | Partial (Management UI) | Needs User Portal UX guide |
| Self-Service Agents | Minimal | Needs comprehensive guide |
| Quotas/Limits | Good technical docs | Needs admin-facing guide |
| Accessibility | Minimal mentions | Needs dedicated section |

---

## Documentation Structure Overview

The existing documentation follows this hierarchy:

```
docs/
├── concepts/                    # Conceptual documentation
│   ├── data-pipeline/          # Data pipeline concepts
│   ├── plugin/                 # Plugin concepts  
│   ├── prompt/                 # Prompt concepts
│   └── quota/                  # Quota concepts
├── setup-guides/               # How-to guides
│   ├── agents/                 # Agent configuration
│   ├── branding/               # Branding customization
│   ├── exposed-apis/           # API documentation
│   ├── management-ui/          # Management Portal guides
│   └── vectorization/          # Vectorization setup
├── operations/                 # Administration guides
├── role-based-access-control/  # RBAC documentation
├── deployment/                 # Deployment guides
├── development/                # Developer guides
├── api/                        # API reference
└── release-notes/              # Release documentation
```

---

## Detailed Topic Plans

### Knowledge Sources

#### 1. Upload Files from SharePoint Online

**Location**: `docs/setup-guides/vectorization/sharepoint-upload-guide.md` (NEW)

**Existing Content**: 
- `docs/setup-guides/vectorization/vectorization-profiles.md` contains SharePoint Online data source configuration
- Entra ID app registration for SharePoint documented

**New Content Required**:
- End-user guide for uploading files from SharePoint Online
- Step-by-step workflow for configuring SharePoint as a knowledge source
- Screenshots of the Management Portal SharePoint configuration
- Troubleshooting common SharePoint connectivity issues

**Cross-References**:
- Link from `docs/setup-guides/vectorization/index.md`
- Link from `docs/setup-guides/vectorization/vectorization-profiles.md`

**[TODO: Obtain screenshots of SharePoint upload UX in Management Portal]**
**[TODO: Confirm supported SharePoint file types and size limits]**
**[TODO: Document any file sync/refresh behavior]**

---

#### 2. Azure Data Lake as a Knowledge Source

**Location**: `docs/setup-guides/vectorization/azure-data-lake-guide.md` (NEW)

**Existing Content**:
- `docs/setup-guides/vectorization/vectorization-profiles.md` contains AzureDataLake data source configuration
- Configuration references documented

**New Content Required**:
- Administrator guide for setting up Azure Data Lake as a knowledge source
- Authentication options (AzureIdentity, AccountKey, ConnectionString)
- Folder structure recommendations
- Best practices for organizing files for vectorization
- Integration with data pipelines

**Cross-References**:
- Link from `docs/setup-guides/vectorization/index.md`
- Link from `docs/concepts/data-pipeline/data-pipeline.md`

**[TODO: Document recommended folder hierarchies for different use cases]**
**[TODO: Clarify ADLS Gen1 vs Gen2 support]**

---

#### 3. Private Storage for Custom Agent Owners

**Location**: `docs/setup-guides/agents/private-storage.md` (NEW)

**Existing Content**: None (new feature)

**New Content Required**:
- Concept explanation: What is private storage for agent owners
- How private storage differs from shared knowledge sources
- Configuration steps via Management Portal
- Configuration steps via Management API
- Access control and permissions
- Storage quotas and limits
- Use cases and best practices

**Cross-References**:
- Link from `docs/setup-guides/agents/index.md`
- Link from self-service agent creation documentation

**[TODO: Confirm private storage implementation details]**
**[TODO: Document storage location (Azure Blob, ADLS, etc.)]**
**[TODO: Clarify access patterns - who can see what]**
**[TODO: Document storage quota configuration]**

---

#### 4. Data Pipeline Creation & Invocation

**Location**: 
- Enhance `docs/concepts/data-pipeline/data-pipeline.md`
- Create `docs/setup-guides/management-ui/data-pipeline-management.md` (NEW)

**Existing Content**:
- `docs/concepts/data-pipeline/data-pipeline.md` has comprehensive technical documentation
- Data pipeline JSON structure documented
- Trigger types documented

**New Content Required**:
- **Management Portal Guide** (NEW):
  - Creating a data pipeline via the UI
  - Configuring pipeline stages
  - Setting up triggers (Manual, Schedule, Event)
  - Running/invoking pipelines
  - Monitoring pipeline progress
  - Reviewing pipeline status and history
  - Troubleshooting failed pipelines
  - Screenshots of all UX steps

**Cross-References**:
- Link from `docs/setup-guides/management-ui/management-ui.md`
- Link from `docs/concepts/data-pipeline/data-pipeline.md`
- Link from `docs/setup-guides/vectorization/index.md`

**[TODO: Obtain screenshots of data pipeline creation UX]**
**[TODO: Document pipeline status states and transitions]**
**[TODO: Clarify real-time monitoring capabilities]**
**[TODO: Document retry behavior for failed stages]**

---

### Data Pipeline

#### 5. Reduced Vectorization Latency

**Location**: 
- Enhance `docs/setup-guides/vectorization/vectorization-configuration.md`
- Add section to `docs/concepts/data-pipeline/data-pipeline.md`

**Existing Content**:
- Vectorization configuration documented
- Async vs sync execution documented

**New Content Required**:
- Performance optimization guide section
- Configuration parameters affecting latency
- Scalability settings (worker instances, parallelization)
- Expected latency benchmarks
- Troubleshooting slow vectorization
- Resource scaling recommendations

**[TODO: Document specific configuration parameters for latency optimization]**
**[TODO: Provide benchmark numbers or ranges]**
**[TODO: Clarify hardware/resource requirements for optimal performance]**

---

### Image Description

#### 6. LLM-Generated Image Description

**Location**: `docs/setup-guides/agents/image-description.md` (NEW)

**Existing Content**:
- Release notes mention multimodal content support
- DALL-E image generation documented in `docs/setup-guides/agents/agents_workflows.md`

**New Content Required**:
- Feature overview: LLM-generated image descriptions
- Supported image formats
- Model size limits and constraints
- How to enable image description for agents
- Configuration options
- Example use cases
- Known limitations

**Cross-References**:
- Link from `docs/setup-guides/agents/index.md`
- Link from `docs/setup-guides/agents/agents_workflows.md`

**[TODO: Confirm which models support image description]**
**[TODO: Document image size/resolution limits]**
**[TODO: Clarify if this is automatic or requires configuration]**
**[TODO: Document token consumption for image processing]**

---

### Knowledge Graph Integration

#### 7. Knowledge Graph as a Knowledge Source

**Location**: `docs/setup-guides/vectorization/knowledge-graph-source.md` (NEW)

**Existing Content**:
- Brief mention in `docs/concepts/index.md` (data pipelines can create knowledge graphs)
- Configuration reference in breaking changes

**New Content Required**:
- Concept: What is a Knowledge Graph in FoundationaLLM
- Setting up Knowledge Graph as a knowledge source
- Connecting agents to Knowledge Graph
- Query patterns and capabilities
- Configuration via Management Portal
- Configuration via API
- Performance considerations
- Use cases and best practices

**Cross-References**:
- Link from `docs/setup-guides/vectorization/index.md`
- Link from `docs/setup-guides/agents/knowledge-management-agent.md`

**[TODO: Document Knowledge Graph data model]**
**[TODO: Clarify supported graph databases (Neo4j, etc.)]**
**[TODO: Document query language/syntax if applicable]**
**[TODO: Confirm current implementation status]**

---

### End-User Agent Management UX

**Location**: `docs/setup-guides/user-portal/agent-management.md` (NEW directory and file)

Create new directory `docs/setup-guides/user-portal/` for User Portal documentation.

#### 9. Managing the Agent "Catalog" Each User Can Access

**New Content Required**:
- Overview of agent catalog concept
- How users can customize their visible agents
- Settings dialog walkthrough
- Persistence of user preferences

#### 10. How Agents Appear in the Agent Dropdown

**New Content Required**:
- Agent dropdown organization
- Featured agents section
- Enabled/disabled states
- Filtering and search functionality

#### 11. Viewing & Managing Agents Created by the User

**New Content Required**:
- Accessing user-created agents
- Agent list views
- Agent status indicators
- Edit/delete capabilities

#### 12. Setting a Default Agent

**New Content Required**:
- How to set a default agent
- Default agent behavior
- Relationship to branding default agent welcome message

#### 13. Clarification: User Portal vs. Management Portal

**New Content Required**:
- Clear delineation of where agent management occurs
- User Portal capabilities vs. Management Portal capabilities
- Decision matrix for which portal to use
- Permissions required for each

**Existing Related Content**:
- `docs/setup-guides/branding/branding-management-portal.md` mentions Default Agent Welcome Message
- `docs/release-notes/release_notes_0.9.7.md` mentions end-user agent selection
- App Configuration: `FoundationaLLM:UserPortal:Configuration:FeaturedAgentNames`

**Cross-References**:
- Link from `docs/setup-guides/index.md`
- Link from `docs/setup-guides/quickstart.md`

**[TODO: Confirm final design decisions for User Portal vs Management Portal agent management]**
**[TODO: Obtain screenshots of User Portal Settings dialog]**
**[TODO: Document featured agents configuration process]**
**[TODO: Clarify agent visibility rules based on RBAC]**

---

### Self-Service Agent Creation UX

**Location**: 
- `docs/setup-guides/user-portal/self-service-agent-creation.md` (NEW)
- `docs/setup-guides/user-portal/agent-sharing-model.md` (NEW)

#### 14. Creating/Editing Custom Agents

**New Content Required**:
- Overview of self-service agent creation
- Prerequisites and permissions required
- Feature flag: `FoundationaLLM.Agent.SelfService`

#### 15. Editable Properties

**Document each property**:

| Property | Description | Status |
|----------|-------------|--------|
| Agent display name | User-facing name | Document |
| Welcome message | Greeting shown to users | Document |
| Prompt definition | System prompt configuration | Document |
| Model selection | Choose LLM model | Document |
| Agent status | Active/expired states | Document |
| Temperature | Model creativity setting | Document |
| Tool selection | Available tools | Document |

**Tool Selection Details**:
- Image generation (DALL-E) - Existing docs in `agents_workflows.md`
- Upload from computer - Document
- Private storage knowledge source - Document (link to private storage docs)
- Website crawler - **[TODO: Document current behavior + future placeholder]**

**Explainer Text per Accessibility**:
- Document accessibility considerations for all form fields
- ARIA labels and descriptions
- Screen reader guidance

#### 16. Agent Sharing Model

**New Content Required**:
- **Owners**: Full permissions (share/edit/delete)
- **Collaborators**: Share and edit permissions
- **Users**: Use only permissions
- How to share agents with others
- Managing sharing settings
- Revoking access

**Cross-References**:
- Link from `docs/role-based-access-control/index.md`
- Link from `docs/setup-guides/agents/index.md`

**[TODO: Confirm sharing model implementation details]**
**[TODO: Document how sharing interacts with existing RBAC]**
**[TODO: Clarify invitation/notification workflow]**
**[TODO: Document sharing limits if any]**

---

### System Status Message

**Location**: `docs/setup-guides/management-ui/system-status-messages.md` (NEW)

#### 17. Publishing Status Messages via Management Portal

**New Content Required**:
- Overview: IT team publishing status/outage/maintenance messages
- Accessing the status message feature in Management Portal
- Creating a new status message
- Message content options (text, HTML, severity levels)
- Scheduling messages
- Editing and removing messages

#### 18. Display Rules for Messages in User Portal

**New Content Required**:
- How messages appear to end users
- Message placement/positioning
- Dismissal behavior
- Priority/severity display differences
- Multi-message handling

**Cross-References**:
- Link from `docs/setup-guides/management-ui/management-ui.md`
- Link from User Portal documentation

**[TODO: Confirm status message feature implementation]**
**[TODO: Obtain screenshots of status message UX]**
**[TODO: Document message severity levels and styling]**
**[TODO: Clarify message scheduling capabilities]**

---

### UX Walk-throughs

**Location**: `docs/setup-guides/user-portal/walkthroughs/` (NEW directory)

#### 19. Full UX Walk-through: Agent Management

**File**: `agent-management-walkthrough.md`

**Content**:
- Step-by-step guide with numbered screenshots
- Opening the Settings dialog
- Viewing available agents
- Enabling/disabling agents
- Using filters
- Setting featured agents
- Saving preferences

#### 20. Full UX Walk-through: Agent Creation

**File**: `agent-creation-walkthrough.md`

**Content**:
- Step-by-step guide with numbered screenshots
- Navigating to agent creation
- Filling out each form section
- Configuring tools
- Setting permissions
- Testing the new agent

#### 21. Full UX Walk-through: Status Message Publishing

**File**: `status-message-walkthrough.md`

**Content**:
- Step-by-step guide with numbered screenshots
- Accessing status message management
- Creating a new message
- Preview and publish
- Editing/removing messages
- Viewing message history

**[TODO: Obtain complete screenshot sets for all walkthroughs]**
**[TODO: Record any video walkthroughs if available]**

---

### API-Level Documentation

**Location**: Enhance existing files in `docs/development/calling-apis/`

#### 22. API-Level Behavior for Agent CRUD Operations

**Enhance**: `docs/development/calling-apis/directly-calling-management-api.md`

**New Content Required**:
- Create agent via API
- Read agent details via API
- Update agent via API
- Delete agent via API
- Sample request/response for each operation
- Error handling

#### 23. Validation Rules, Error Messages, Accessibility, Permissions

**Location**: `docs/development/calling-apis/api-validation-errors.md` (NEW)

**New Content Required**:
- Common validation rules
- Error response formats
- Error code reference table
- Permission requirements per operation
- Accessibility considerations for API consumers

**[TODO: Compile complete list of API validation rules]**
**[TODO: Document all error codes and messages]**
**[TODO: Clarify rate limiting behavior during errors]**

---

### Core API / Platform

#### 26. OpenAI Model Endpoint Facades

**Location**: `docs/concepts/openai-endpoint-facades.md` (NEW)

**New Content Required**:
- Concept: What are endpoint facades
- Why use facades (abstraction, load balancing, failover)
- Configuration options
- Supported OpenAI operations
- Monitoring and metrics

**[TODO: Confirm facade implementation status - noted as "still under development"]**
**[TODO: Document supported operations when available]**
**[TODO: Clarify deployment model and configuration]**

---

#### 27. Token Usage Limits

**Location**: Enhance `docs/concepts/quota/quota-definition.md`

**Existing Content**:
- Good documentation on quota definitions
- Request rate limits documented

**New Content Required**:
- Token-specific limits (distinct from request rate)
- Per-user token limits
- Per-agent token limits
- Token counting methodology
- Monitoring token usage
- User-facing token limit messages

**Cross-References**:
- `FoundationaLLM:APIEndpoints:GatewayAPI:Configuration:TokenRateLimitMultiplier` in breaking changes

**[TODO: Clarify token limit enforcement mechanism]**
**[TODO: Document user notification for token limit approach]**

---

#### 28. API Key Limits

**Location**: `docs/concepts/quota/api-key-limits.md` (NEW)

**New Content Required**:
- API key generation limits
- API key usage limits
- Key rotation policies
- Key expiration behavior
- Managing keys via Management Portal
- Managing keys via API

**[TODO: Document API key limit configuration]**
**[TODO: Clarify key scope (instance, user, agent)]**

---

#### 29. API Quota/Rate Limits

**Location**: Enhance existing `docs/concepts/quota/` documentation

**Existing Content**:
- `quota-definition.md` - Comprehensive
- `api-raw-request-rate.md` - Comprehensive
- `agent-request-rate.md` - Comprehensive

**New Content Required**:
- Administrator-facing summary guide
- How to configure quotas via Management Portal
- Recommended quota settings for different deployment sizes
- Monitoring quota usage
- Responding to quota exhaustion

**Cross-References**:
- Link from `docs/operations/index.md`

---

### Accessibility Requirements

#### 30. WCAG Compliance in User Portal

**Location**: `docs/setup-guides/user-portal/accessibility.md` (NEW)

**Existing Content**:
- `docs/setup-guides/branding/branding-management-portal.md` mentions WCAG contrast ratios
- Release notes mention accessibility improvements

**New Content Required**:
- Overview of accessibility commitment
- WCAG 2.1 compliance level (AA target)
- Keyboard navigation support
- Screen reader compatibility
- Color contrast requirements
- Focus indicators
- ARIA labels and landmarks
- Form accessibility
- Error message accessibility
- Known accessibility limitations
- Accessibility testing procedures
- Reporting accessibility issues

**User Portal Specific Features**:
- Agent dropdown accessibility
- Chat interface accessibility
- File upload accessibility
- Settings dialog accessibility
- Message rating accessibility

**Cross-References**:
- Link from `docs/setup-guides/branding/index.md`
- Link from `docs/setup-guides/user-portal/index.md`

**[TODO: Conduct accessibility audit of User Portal]**
**[TODO: Document specific ARIA patterns used]**
**[TODO: List known accessibility issues and remediation timeline]**
**[TODO: Document keyboard shortcut reference]**

---

## TODO Items Summary

### High Priority (Blocking Documentation)

| # | Topic | TODO Item |
|---|-------|-----------|
| 1 | Private Storage | Confirm implementation details and storage location |
| 2 | Self-Service Agents | Confirm sharing model implementation |
| 3 | System Status Messages | Confirm feature implementation status |
| 4 | OpenAI Facades | Confirm development status and scope |
| 5 | Knowledge Graph | Confirm current implementation status |

### Medium Priority (Requires Information)

| # | Topic | TODO Item |
|---|-------|-----------|
| 6 | SharePoint | Obtain UX screenshots, confirm file limits |
| 7 | Data Pipelines | Obtain monitoring UX screenshots |
| 8 | Image Description | Confirm supported models and limits |
| 9 | User Portal | Obtain Settings dialog screenshots |
| 10 | Agent Creation | Obtain complete walkthrough screenshots |
| 11 | Website Crawler | Document current behavior + placeholder |

### Lower Priority (Enhancement)

| # | Topic | TODO Item |
|---|-------|-----------|
| 12 | Azure Data Lake | Document folder structure recommendations |
| 13 | Vectorization Latency | Provide benchmark numbers |
| 14 | API Validation | Compile complete error code list |
| 15 | Token Limits | Clarify enforcement mechanism |
| 16 | Accessibility | Conduct formal accessibility audit |

---

## Implementation Priority

### Phase 1: Core User Portal Documentation
1. Create `docs/setup-guides/user-portal/` directory structure
2. Document agent management UX (Topics 9-13)
3. Document self-service agent creation (Topics 14-16)
4. Create UX walkthroughs (Topics 19-21)

### Phase 2: Knowledge Sources
1. SharePoint Online upload guide (Topic 1)
2. Azure Data Lake guide (Topic 2)
3. Private storage documentation (Topic 3)
4. Data pipeline monitoring guide (Topic 4)

### Phase 3: API & Platform
1. API CRUD documentation (Topics 22-23)
2. Quota/limits admin guide (Topics 27-29)
3. OpenAI facades (Topic 26) - when development complete

### Phase 4: Advanced Features
1. Image description (Topic 6)
2. Knowledge Graph integration (Topic 7)
3. Reduced vectorization latency (Topic 5)

### Phase 5: Compliance & Operations
1. Accessibility documentation (Topic 30)
2. System status messages (Topics 17-18)

---

## New Files to Create

| File Path | Topic(s) |
|-----------|----------|
| `docs/setup-guides/user-portal/index.md` | 9-13, 30 |
| `docs/setup-guides/user-portal/agent-management.md` | 9-13 |
| `docs/setup-guides/user-portal/self-service-agent-creation.md` | 14-15 |
| `docs/setup-guides/user-portal/agent-sharing-model.md` | 16 |
| `docs/setup-guides/user-portal/accessibility.md` | 30 |
| `docs/setup-guides/user-portal/walkthroughs/agent-management-walkthrough.md` | 19 |
| `docs/setup-guides/user-portal/walkthroughs/agent-creation-walkthrough.md` | 20 |
| `docs/setup-guides/user-portal/walkthroughs/status-message-walkthrough.md` | 21 |
| `docs/setup-guides/vectorization/sharepoint-upload-guide.md` | 1 |
| `docs/setup-guides/vectorization/azure-data-lake-guide.md` | 2 |
| `docs/setup-guides/vectorization/knowledge-graph-source.md` | 7 |
| `docs/setup-guides/agents/private-storage.md` | 3 |
| `docs/setup-guides/agents/image-description.md` | 6 |
| `docs/setup-guides/management-ui/data-pipeline-management.md` | 4 |
| `docs/setup-guides/management-ui/system-status-messages.md` | 17-18 |
| `docs/concepts/openai-endpoint-facades.md` | 26 |
| `docs/concepts/quota/api-key-limits.md` | 28 |
| `docs/development/calling-apis/api-validation-errors.md` | 23 |

## Files to Enhance

| File Path | Topic(s) |
|-----------|----------|
| `docs/toc.yml` | Add new files to navigation |
| `docs/setup-guides/index.md` | Add User Portal section |
| `docs/setup-guides/vectorization/index.md` | Add new guide links |
| `docs/setup-guides/vectorization/vectorization-configuration.md` | 5 |
| `docs/concepts/data-pipeline/data-pipeline.md` | 4, 5 |
| `docs/concepts/quota/quota-definition.md` | 27 |
| `docs/development/calling-apis/directly-calling-management-api.md` | 22 |
| `docs/operations/index.md` | Add quota management links |

---

*Document created: Documentation Update Plan*
*Last updated: [Current Date]*
