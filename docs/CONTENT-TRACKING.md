# Content Tracking for Documentation Reorganization

This document tracks all content affected by the documentation reorganization for review purposes.

## Quick Stats

- **Total Existing Files**: 82 markdown files
- **Files to Move**: 65
- **Files to Archive (Obsolete)**: 8
- **Files with Partial Extraction**: 4
- **New Stubs to Create**: 52
- **Files Staying in Place**: 5

---

## ✅ Content Moved (Full File Moves)

These files are moved completely to their new location. Links and references may need updating.

### Deployment → Platform Operations

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `deployment/index.md` | `platform-operations/deployment/index.md` | ⬜ Pending |
| `deployment/deployment-quick-start.md` | `platform-operations/deployment/deployment-quick-start.md` | ⬜ Pending |
| `deployment/deployment-standard.md` | `platform-operations/deployment/deployment-standard.md` | ⬜ Pending |
| `deployment/deployment-configuration.md` | `platform-operations/deployment/deployment-configuration.md` | ⬜ Pending |
| `deployment/app-configuration-values.md` | `platform-operations/deployment/app-configuration-values.md` | ⬜ Pending |
| `deployment/azure-resource-providers-requirements.md` | `platform-operations/deployment/azure-resource-providers-requirements.md` | ⬜ Pending |
| `deployment/custom-domains.md` | `platform-operations/deployment/custom-domains.md` | ⬜ Pending |
| `deployment/soft-delete.md` | `platform-operations/deployment/soft-delete.md` | ⬜ Pending |
| `deployment/standard/manifest.md` | `platform-operations/deployment/standard-manifest.md` | ⬜ Pending |
| `deployment/configure-access-control-for-services.md` | `platform-operations/security-permissions/configure-access-control-services.md` | ⬜ Pending |

### Authentication/Authorization → Platform Operations

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `deployment/authentication-authorization/index.md` | `platform-operations/security-permissions/authentication-authorization/index.md` | ⬜ Pending |
| `deployment/authentication-authorization/core-authentication-setup-entra.md` | `platform-operations/security-permissions/authentication-authorization/pre-deployment/core-authentication-setup.md` | ⬜ Pending |
| `deployment/authentication-authorization/management-authentication-setup-entra.md` | `platform-operations/security-permissions/authentication-authorization/pre-deployment/management-authentication-setup.md` | ⬜ Pending |
| `deployment/authentication-authorization/authorization-setup-entra.md` | `platform-operations/security-permissions/authentication-authorization/pre-deployment/authorization-setup.md` | ⬜ Pending |
| `deployment/authentication-authorization/post-core-deployment.md` | `platform-operations/security-permissions/authentication-authorization/post-deployment/core-authentication-post.md` | ⬜ Pending |
| `deployment/authentication-authorization/post-management-deployment.md` | `platform-operations/security-permissions/authentication-authorization/post-deployment/management-authentication-post.md` | ⬜ Pending |
| `deployment/authentication-authorization/post-authorization-deployment.md` | `platform-operations/security-permissions/authentication-authorization/post-deployment/authorization-post.md` | ⬜ Pending |

### Operations → Platform Operations

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `operations/security.md` | `platform-operations/security-permissions/platform-security.md` | ⬜ Pending |
| `operations/graph-api-permissions.md` | `platform-operations/security-permissions/graph-api-permissions.md` | ⬜ Pending |
| `operations/network-security-groups.md` | `platform-operations/security-permissions/network-security-groups.md` | ⬜ Pending |
| `operations/vulnerabilities.md` | `platform-operations/security-permissions/vulnerabilities.md` | ⬜ Pending |
| `operations/logs.md` | `platform-operations/monitoring-troubleshooting/logs.md` | ⬜ Pending |
| `operations/troubleshooting.md` | `platform-operations/monitoring-troubleshooting/troubleshooting.md` | ⬜ Pending |
| `operations/update.md` | `platform-operations/how-to-guides/updating-container-versions.md` | ⬜ Pending |
| `operations/backups.md` | `platform-operations/how-to-guides/backups.md` | ⬜ Pending |
| `operations/purge-conversations.md` | `platform-operations/how-to-guides/purge-conversations.md` | ⬜ Pending |
| `operations/release-notes.md` | `platform-operations/how-to-guides/creating-release-notes.md` | ⬜ Pending |

### RBAC → Platform Operations

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `role-based-access-control/index.md` | `platform-operations/security-permissions/role-based-access-control/index.md` | ⬜ Pending |
| `role-based-access-control/role-definitions.md` | `platform-operations/security-permissions/role-based-access-control/role-definitions.md` | ⬜ Pending |
| `role-based-access-control/role-assignments.md` | `platform-operations/security-permissions/role-based-access-control/role-assignments.md` | ⬜ Pending |
| `role-based-access-control/scope.md` | `platform-operations/security-permissions/role-based-access-control/scope.md` | ⬜ Pending |
| `role-based-access-control/role-management.md` | `platform-operations/security-permissions/role-based-access-control/role-management.md` | ⬜ Pending |
| `role-based-access-control/agent-role-assignments.md` | `platform-operations/security-permissions/role-based-access-control/agent-role-assignments.md` | ⬜ Pending |

### Setup Guides → Various Sections

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `setup-guides/quickstart.md` | `chat-user-portal/quick-start/quickstart.md` | ⬜ Pending |
| `setup-guides/management-ui/management-ui.md` | `management-portal/index.md` | ⬜ Pending |
| `setup-guides/agents/agents_workflows.md` | `management-portal/reference/concepts/agents-workflows.md` | ⬜ Pending |
| `setup-guides/agents/Agent_AccessToken.md` | `management-portal/reference/concepts/agent-access-tokens.md` | ⬜ Pending |
| `setup-guides/agents/prompt-resource.md` | `management-portal/reference/concepts/prompts-resources.md` | ⬜ Pending |
| `setup-guides/exposed-apis/core-api.md` | `apis-sdks/apis/core-api/index.md` | ⬜ Pending |
| `setup-guides/exposed-apis/management-api.md` | `apis-sdks/apis/management-api/index.md` | ⬜ Pending |
| `setup-guides/exposed-apis/resource-management/resource-management.md` | `management-portal/reference/concepts/resource-management.md` | ⬜ Pending |
| `setup-guides/branding/index.md` | `management-portal/reference/branding/index.md` | ⬜ Pending |
| `setup-guides/branding/branding-app-configuration.md` | `management-portal/reference/branding/using-app-configuration.md` | ⬜ Pending |
| `setup-guides/branding/branding-management-portal.md` | `management-portal/reference/branding/using-management-portal.md` | ⬜ Pending |

### How-To Guides → Management Portal

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `how-to-guides/create-model-agnostic-agent-claude.md` | `management-portal/how-to-guides/agents/create-model-agnostic-agent-claude.md` | ⬜ Pending |
| `how-to-guides/create-model-agnostic-agent-gpt4o.md` | `management-portal/how-to-guides/agents/create-model-agnostic-agent-gpt4o.md` | ⬜ Pending |

### Development → APIs/SDKs + Development

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `development/index.md` | `development/index.md` | ⬜ Pending (structure same) |
| `development/development-approach.md` | `development/development-approach.md` | ⬜ Pending (structure same) |
| `development/development-local.md` | `development/development-local.md` | ⬜ Pending (structure same) |
| `development/calling-apis/directly-calling-core-api.md` | `apis-sdks/apis/core-api/directly-calling-core-api.md` | ⬜ Pending |
| `development/calling-apis/directly-calling-management-api.md` | `apis-sdks/apis/management-api/directly-calling-management-api.md` | ⬜ Pending |
| `development/calling-apis/standard-deployment-local-api-access.md` | `apis-sdks/apis/core-api/standard-deployment-local-api-access.md` | ⬜ Pending |
| `development/contributing/index.md` | `development/contributing/index.md` | ⬜ Pending (structure same) |
| `development/contributing/git-workflow.md` | `development/contributing/git-workflow.md` | ⬜ Pending (structure same) |
| `development/contributing/style-guide.md` | `development/contributing/style-guide.md` | ⬜ Pending (structure same) |
| `development/contributing/repro.md` | `development/contributing/bug-report-reproduction.md` | ⬜ Pending |

### API Documentation → APIs/SDKs

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `api/index.md` | `apis-sdks/apis/core-api/api-reference.md` | ⬜ Pending |
| `api/dotnet/index.md` | `apis-sdks/sdks/dotnet/index.md` | ⬜ Pending |
| `api/python/index.md` | `apis-sdks/sdks/python/index.md` | ⬜ Pending |

### Concepts → Management Portal Reference

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `concepts/data-pipeline/data-pipeline.md` | `management-portal/reference/concepts/data-pipelines.md` | ⬜ Pending |
| `concepts/plugin/plugin.md` | `management-portal/reference/concepts/plugins-packages.md` (merged) | ⬜ Pending |
| `concepts/plugin/plugin-package.md` | `management-portal/reference/concepts/plugins-packages.md` (merged) | ⬜ Pending |
| `concepts/quota/quota-definition.md` | `management-portal/reference/concepts/quotas.md` (merged) | ⬜ Pending |
| `concepts/quota/agent-request-rate.md` | `management-portal/reference/concepts/quotas.md` (merged) | ⬜ Pending |
| `concepts/quota/api-raw-request-rate.md` | `management-portal/reference/concepts/quotas.md` (merged) | ⬜ Pending |

### Reference Materials → Reference

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `schema.md` | `reference/agent-schemas.md` | ⬜ Pending |
| `documentation-generation.md` | `reference/documentation-generation.md` | ⬜ Pending |
| `release-notes/breaking-changes.md` | `reference/release-notes/breaking-changes.md` | ⬜ Pending |
| `release-notes/release_notes_0.9.7.md` | `reference/release-notes/release_notes_0.9.7.md` | ⬜ Pending |

### Root Level → Overview

| Original Path | New Path | Review Status |
|---------------|----------|---------------|
| `index.md` | `overview/index.md` | ⬜ Pending |

---

## 📋 Content Requiring Partial Extraction

These files have content that needs to be extracted to new files while keeping some content in place.

| Source File | Content to Extract | Destination File | Review Status |
|-------------|-------------------|------------------|---------------|
| `index.md` | "Why is FoundationaLLM Needed?" section + "Where can FoundationaLLM fill the need?" section | `overview/why-foundationallm.md` | ⬜ Needs Review |
| `concepts/index.md` | Architecture mindmap and core concepts overview | `overview/architecture-concepts.md` | ⬜ Needs Review |
| `setup-guides/quickstart.md` | "Find your Core API URL" section | `apis-sdks/apis/core-api/finding-core-api-url.md` | ⬜ Needs Review |
| `setup-guides/management-ui/management-ui.md` | Agent creation tutorial section | `management-portal/how-to-guides/agents/create-new-agent.md` | ⬜ Needs Review |

---

## 🗄️ Content Archived (Obsolete)

These files are marked as obsolete per the outline and should be moved to an archive folder.

| Original Path | Archive Path | Reason | Review Status |
|---------------|--------------|--------|---------------|
| `setup-guides/vectorization/index.md` | `archive/vectorization/index.md` | Vectorization section dropped | ⬜ Pending |
| `setup-guides/vectorization/vectorization-concepts.md` | `archive/vectorization/vectorization-concepts.md` | Vectorization section dropped | ⬜ Pending |
| `setup-guides/vectorization/vectorization-configuration.md` | `archive/vectorization/vectorization-configuration.md` | Vectorization section dropped | ⬜ Pending |
| `setup-guides/vectorization/vectorization-profiles.md` | `archive/vectorization/vectorization-profiles.md` | Vectorization section dropped | ⬜ Pending |
| `setup-guides/vectorization/vectorization-triggering.md` | `archive/vectorization/vectorization-triggering.md` | Vectorization section dropped | ⬜ Pending |
| `setup-guides/vectorization/vectorization-monitoring-troubleshooting.md` | `archive/vectorization/vectorization-monitoring-troubleshooting.md` | Vectorization section dropped | ⬜ Pending |
| `setup-guides/agents/knowledge-management-agent.md` | `archive/knowledge-management-agent.md` | Marked [OBSOLETE] in outline | ⬜ Pending |
| `development/calling-apis/directly-calling-vectorization-api.md` | `archive/directly-calling-vectorization-api.md` | Vectorization API marked [OBSOLETE] | ⬜ Pending |

---

## ✨ New Content Required (Stubs to Create)

These are new files that need to be created as stubs for future content.

### Chat User Portal - How-To Guides

| File Path | Description | Priority |
|-----------|-------------|----------|
| `chat-user-portal/index.md` | Chat User Portal overview | High |
| `chat-user-portal/quick-start/creating-first-agent.md` | Creating first agent guide | High |
| `chat-user-portal/how-to-guides/using-agents/selecting-agent.md` | How to select an agent | Medium |
| `chat-user-portal/how-to-guides/using-agents/managing-available-agents.md` | Managing available agents | Medium |
| `chat-user-portal/how-to-guides/using-agents/managing-conversations.md` | Managing conversations | Medium |
| `chat-user-portal/how-to-guides/using-agents/configuring-accessibility.md` | Accessibility configuration | Low |
| `chat-user-portal/how-to-guides/using-agents/uploading-files.md` | Uploading files to conversation | High |
| `chat-user-portal/how-to-guides/using-agents/downloading-files.md` | Downloading files from conversation | Medium |
| `chat-user-portal/how-to-guides/using-agents/using-code-interpreter.md` | Using code interpreter tool | High |
| `chat-user-portal/how-to-guides/using-agents/using-knowledge-tool.md` | Using knowledge tool | High |
| `chat-user-portal/how-to-guides/using-agents/using-other-tools.md` | Using other tools | Medium |
| `chat-user-portal/how-to-guides/using-agents/monitoring-tokens.md` | Monitoring token consumption | Low |
| `chat-user-portal/how-to-guides/using-agents/rating-responses.md` | Rating agent responses | Low |
| `chat-user-portal/how-to-guides/using-agents/copying-prompts-results.md` | Copying prompts & formatted results | Medium |
| `chat-user-portal/how-to-guides/using-agents/printing-conversations.md` | Printing conversations | Low |
| `chat-user-portal/how-to-guides/using-agents/viewing-agent-prompts.md` | Viewing agent prompts | Low |

### Management Portal - How-To Guides

| File Path | Description | Priority |
|-----------|-------------|----------|
| `management-portal/quick-start/portal-tour.md` | Tour of the portal | High |
| `management-portal/quick-start/creating-first-agent.md` | Creating first agent | High |
| `management-portal/how-to-guides/agents/create-new-agent.md` | Create new agent guide | High |
| `management-portal/how-to-guides/agents/all-agents.md` | All agents view | Medium |
| `management-portal/how-to-guides/agents/my-agents.md` | My agents view | Medium |
| `management-portal/how-to-guides/agents/prompts.md` | Managing prompts | Medium |
| `management-portal/how-to-guides/data/data-sources.md` | Data sources guide | High |
| `management-portal/how-to-guides/data/data-pipelines/creating-data-pipelines.md` | Creating data pipelines | High |
| `management-portal/how-to-guides/data/data-pipelines/invoking-data-pipelines.md` | Invoking data pipelines | High |
| `management-portal/how-to-guides/data/data-pipelines/monitoring-data-pipelines.md` | Monitoring data pipelines | Medium |
| `management-portal/how-to-guides/data/data-pipeline-runs.md` | Data pipeline runs | Medium |
| `management-portal/how-to-guides/data/knowledge-sources/sharepoint-online.md` | SharePoint Online source | Medium |
| `management-portal/how-to-guides/data/knowledge-sources/azure-data-lake.md` | Azure Data Lake source | Medium |
| `management-portal/how-to-guides/data/knowledge-sources/private-storage.md` | Private storage for agents | Medium |
| `management-portal/how-to-guides/data/knowledge-sources/knowledge-graph-integration.md` | Knowledge graph integration | Low |
| `management-portal/how-to-guides/data/knowledge-sources/image-description.md` | Image-to-text description | Low |
| `management-portal/how-to-guides/models-endpoints/ai-models.md` | Managing AI models | High |
| `management-portal/how-to-guides/models-endpoints/api-endpoints.md` | Managing API endpoints | Medium |
| `management-portal/how-to-guides/security/instance-access-control.md` | Instance access control | High |
| `management-portal/how-to-guides/fllm-platform/branding.md` | Branding configuration | Low |
| `management-portal/how-to-guides/fllm-platform/configuration.md` | Platform configuration | Medium |
| `management-portal/how-to-guides/fllm-platform/deployment-information.md` | Deployment information | Low |
| `management-portal/how-to-guides/managing-plugins.md` | Managing plugins | Medium |
| `management-portal/how-to-guides/configuring-quotas.md` | Configuring quotas | Medium |

### Management Portal - Reference

| File Path | Description | Priority |
|-----------|-------------|----------|
| `management-portal/reference/branding/using-rest-api.md` | Branding via REST API | Low |
| `management-portal/reference/configuration-reference.md` | Configuration reference (from code) | Medium |
| `management-portal/reference/permissions-roles.md` | Permissions & roles reference | High |

### APIs & SDKs

| File Path | Description | Priority |
|-----------|-------------|----------|
| `apis-sdks/apis/core-api/finding-core-api-url.md` | Finding Core API URL | High |
| `apis-sdks/apis/management-api/resource-providers-overview.md` | Resource providers overview | Medium |
| `apis-sdks/apis/management-api/api-reference.md` | Management API reference | High |
| `apis-sdks/apis/management-api/data-pipelines.md` | Data pipelines API | Medium |

### Reference

| File Path | Description | Priority |
|-----------|-------------|----------|
| `reference/release-notes/index.md` | Release notes index | Low |

---

## 📁 Files Staying in Place

These files are not moving but may need link updates.

| File | Reason |
|------|--------|
| `concepts/index.md` | Core concepts hub (content extracted but file stays) |
| `concepts/prompt/prompt-variable.md` | Referenced by concepts |
| `docfx.json` | Configuration file |
| `.gitignore` | Git configuration |
| `.ignore` | Search ignore |

---

## 🔗 Links Requiring Update

After reorganization, these link patterns need to be updated throughout the documentation:

| Old Pattern | New Pattern |
|-------------|-------------|
| `../deployment/` | `../platform-operations/deployment/` |
| `../operations/` | `../platform-operations/` (distributed) |
| `../setup-guides/agents/` | `../management-portal/reference/concepts/` |
| `../setup-guides/vectorization/` | `../archive/vectorization/` |
| `../role-based-access-control/` | `../platform-operations/security-permissions/role-based-access-control/` |
| `../development/calling-apis/` | `../apis-sdks/apis/` |
| `../api/` | `../apis-sdks/` |
| `../how-to-guides/` | `../management-portal/how-to-guides/agents/` |

---

## 📝 Review Checklist

Use this checklist when reviewing each section:

- [ ] All files moved to correct location
- [ ] Media files moved/updated
- [ ] Internal links updated
- [ ] Cross-references verified
- [ ] Frontmatter/metadata updated (if applicable)
- [ ] TOC entry added
- [ ] Build tested (docfx)
- [ ] Content accurate and up-to-date

---

## Progress Tracking

| Section | Files Moved | Links Updated | Reviewed | Complete |
|---------|-------------|---------------|----------|----------|
| Overview | ⬜ | ⬜ | ⬜ | ⬜ |
| Chat User Portal | ⬜ | ⬜ | ⬜ | ⬜ |
| Management Portal | ⬜ | ⬜ | ⬜ | ⬜ |
| APIs & SDKs | ⬜ | ⬜ | ⬜ | ⬜ |
| Platform Operations | ⬜ | ⬜ | ⬜ | ⬜ |
| Development | ⬜ | ⬜ | ⬜ | ⬜ |
| Reference | ⬜ | ⬜ | ⬜ | ⬜ |
| Archive | ⬜ | ⬜ | ⬜ | ⬜ |

---

## Reorganization Status: COMPLETED

The documentation reorganization has been executed. All phases are complete:

- ✅ Phase 1: Created new folder structure
- ✅ Phase 2: Moved existing files to new locations
- ✅ Phase 3: Extracted partial content to new files
- ✅ Phase 4: Created stub files for new content (52 stubs)
- ✅ Phase 5: Archived obsolete content (vectorization, knowledge-management-agent)
- ✅ Phase 6: Merged files (plugins, quotas)
- ✅ Phase 7: Updated toc.yml for new structure

**Total files in new structure: 119 markdown files**

---

## Cleanup Status: COMPLETED

Legacy folders have been cleaned up and archived:

- ✅ Migrated `concepts/index.md` → `management-portal/reference/concepts/index.md`
- ✅ Migrated `concepts/prompt/prompt-variable.md` → `management-portal/reference/concepts/prompt-variables.md`
- ✅ Archived `api/` → `archive/api/`
- ✅ Archived `concepts/` → `archive/concepts/`
- ✅ Archived `deployment/` → `archive/deployment/`
- ✅ Archived `how-to-guides/` → `archive/how-to-guides/`
- ✅ Archived `operations/` → `archive/operations/`
- ✅ Archived `role-based-access-control/` → `archive/role-based-access-control/`
- ✅ Archived `setup-guides/` → `archive/setup-guides/`
- ✅ Kept `release-notes/` at root level (top-level section)
- ✅ Updated `archive/README.md` with all archived content

---

*Reorganization Completed: December 14, 2025*
*Cleanup Completed: December 14, 2025*
*Plan Version: 1.1*
