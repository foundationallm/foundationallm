# FoundationaLLM Documentation Reorganization Plan

This document outlines the plan to reorganize the documentation based on the structure defined in `outline.md`.

## Summary

| Category | Count |
|----------|-------|
| Files to Move (Full) | 45 |
| Files to Copy (Partial Content) | 3 |
| New Files to Create (Stubs) | 52 |
| Files to Mark as Obsolete | 8 |
| Folders to Create | 25 |

---

## New Folder Structure

```
docs/
├── overview/
│   ├── index.md
│   ├── architecture-concepts.md
│   └── why-foundationallm.md
│
├── chat-user-portal/
│   ├── index.md
│   ├── quick-start/
│   │   ├── quickstart.md
│   │   └── creating-first-agent.md
│   └── how-to-guides/
│       └── using-agents/
│           ├── selecting-agent.md
│           ├── managing-available-agents.md
│           ├── managing-conversations.md
│           ├── configuring-accessibility.md
│           ├── uploading-files.md
│           ├── downloading-files.md
│           ├── using-code-interpreter.md
│           ├── using-knowledge-tool.md
│           ├── using-other-tools.md
│           ├── monitoring-tokens.md
│           ├── rating-responses.md
│           ├── copying-prompts-results.md
│           ├── printing-conversations.md
│           └── viewing-agent-prompts.md
│
├── management-portal/
│   ├── index.md
│   ├── quick-start/
│   │   ├── portal-tour.md
│   │   └── creating-first-agent.md
│   ├── how-to-guides/
│   │   ├── agents/
│   │   │   ├── create-new-agent.md
│   │   │   ├── create-model-agnostic-agent-claude.md
│   │   │   ├── create-model-agnostic-agent-gpt4o.md
│   │   │   ├── all-agents.md
│   │   │   ├── my-agents.md
│   │   │   └── prompts.md
│   │   ├── data/
│   │   │   ├── data-sources.md
│   │   │   ├── data-pipelines/
│   │   │   │   ├── creating-data-pipelines.md
│   │   │   │   ├── invoking-data-pipelines.md
│   │   │   │   └── monitoring-data-pipelines.md
│   │   │   ├── data-pipeline-runs.md
│   │   │   └── knowledge-sources/
│   │   │       ├── sharepoint-online.md
│   │   │       ├── azure-data-lake.md
│   │   │       ├── private-storage.md
│   │   │       ├── knowledge-graph-integration.md
│   │   │       └── image-description.md
│   │   ├── models-endpoints/
│   │   │   ├── ai-models.md
│   │   │   └── api-endpoints.md
│   │   ├── security/
│   │   │   └── instance-access-control.md
│   │   ├── fllm-platform/
│   │   │   ├── branding.md
│   │   │   ├── configuration.md
│   │   │   └── deployment-information.md
│   │   ├── managing-plugins.md
│   │   └── configuring-quotas.md
│   └── reference/
│       ├── concepts/
│       │   ├── agents-workflows.md
│       │   ├── agent-access-tokens.md
│       │   ├── prompts-resources.md
│       │   ├── knowledge-management-agent.md  (OBSOLETE)
│       │   ├── resource-management.md
│       │   ├── data-pipelines.md
│       │   ├── plugins-packages.md
│       │   ├── vectorization.md  (OBSOLETE)
│       │   └── quotas.md
│       ├── branding/
│       │   ├── index.md
│       │   ├── using-app-configuration.md
│       │   ├── using-management-portal.md
│       │   └── using-rest-api.md
│       ├── configuration-reference.md
│       └── permissions-roles.md
│
├── apis-sdks/
│   ├── apis/
│   │   ├── core-api/
│   │   │   ├── index.md
│   │   │   ├── finding-core-api-url.md
│   │   │   ├── directly-calling-core-api.md
│   │   │   ├── standard-deployment-local-api-access.md
│   │   │   └── api-reference.md
│   │   └── management-api/
│   │       ├── index.md
│   │       ├── resource-providers-overview.md
│   │       ├── directly-calling-management-api.md
│   │       ├── api-reference.md
│   │       └── data-pipelines.md
│   └── sdks/
│       ├── dotnet/
│       │   └── index.md
│       └── python/
│           └── index.md
│
├── platform-operations/
│   ├── deployment/
│   │   ├── index.md
│   │   ├── deployment-quick-start.md
│   │   ├── deployment-standard.md
│   │   ├── deployment-configuration.md
│   │   ├── app-configuration-values.md
│   │   ├── azure-resource-providers-requirements.md
│   │   ├── custom-domains.md
│   │   ├── soft-delete.md
│   │   └── standard-manifest.md
│   ├── security-permissions/
│   │   ├── platform-security.md
│   │   ├── authentication-authorization/
│   │   │   ├── index.md
│   │   │   ├── pre-deployment/
│   │   │   │   ├── core-authentication-setup.md
│   │   │   │   ├── management-authentication-setup.md
│   │   │   │   └── authorization-setup.md
│   │   │   └── post-deployment/
│   │   │       ├── core-authentication-post.md
│   │   │       ├── management-authentication-post.md
│   │   │       └── authorization-post.md
│   │   ├── role-based-access-control/
│   │   │   ├── index.md
│   │   │   ├── role-definitions.md
│   │   │   ├── role-assignments.md
│   │   │   ├── scope.md
│   │   │   ├── role-management.md
│   │   │   └── agent-role-assignments.md
│   │   ├── configure-access-control-services.md
│   │   ├── graph-api-permissions.md
│   │   ├── network-security-groups.md
│   │   └── vulnerabilities.md
│   ├── monitoring-troubleshooting/
│   │   ├── logs.md
│   │   └── troubleshooting.md
│   └── how-to-guides/
│       ├── updating-container-versions.md
│       ├── backups.md
│       ├── purge-conversations.md
│       └── creating-release-notes.md
│
├── development/
│   ├── index.md
│   ├── development-approach.md
│   ├── development-local.md
│   └── contributing/
│       ├── index.md
│       ├── git-workflow.md
│       ├── style-guide.md
│       └── bug-report-reproduction.md
│
├── reference/
│   ├── agent-schemas.md
│   ├── release-notes/
│   │   ├── index.md
│   │   ├── breaking-changes.md
│   │   └── [version-specific files]
│   └── documentation-generation.md
│
└── archive/  (for obsolete content)
    └── vectorization/
        ├── index.md
        ├── vectorization-concepts.md
        ├── vectorization-configuration.md
        ├── vectorization-profiles.md
        ├── vectorization-triggering.md
        └── vectorization-monitoring-troubleshooting.md
```

---

## Detailed File Mapping

### Legend
- 🔄 **MOVE** - Move existing file to new location
- 📋 **COPY** - Copy subset of content to new file
- ✨ **CREATE** - New file needs to be created (stub)
- 🗄️ **ARCHIVE** - Move to archive as obsolete
- ❌ **DROP** - Content marked for removal (per outline)

---

## 1. Overview Section

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `overview/index.md` | `index.md` | 🔄 MOVE | Main landing page |
| `overview/architecture-concepts.md` | `concepts/index.md` | 📋 COPY | Extract architecture overview; keep concepts in original |
| `overview/why-foundationallm.md` | `index.md` | 📋 COPY | Extract "Why FoundationaLLM?" section |

---

## 2. Chat User Portal Section

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `chat-user-portal/index.md` | - | ✨ CREATE | New overview of Chat User Portal |
| `chat-user-portal/quick-start/quickstart.md` | `setup-guides/quickstart.md` | 🔄 MOVE | Full file move |
| `chat-user-portal/quick-start/creating-first-agent.md` | - | ✨ CREATE | New stub for first agent guide |
| `chat-user-portal/how-to-guides/using-agents/selecting-agent.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/managing-available-agents.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/managing-conversations.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/configuring-accessibility.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/uploading-files.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/downloading-files.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/using-code-interpreter.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/using-knowledge-tool.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/using-other-tools.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/monitoring-tokens.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/rating-responses.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/copying-prompts-results.md` | - | ✨ CREATE | Includes Dual-Format Copy behavior |
| `chat-user-portal/how-to-guides/using-agents/printing-conversations.md` | - | ✨ CREATE | New how-to guide |
| `chat-user-portal/how-to-guides/using-agents/viewing-agent-prompts.md` | - | ✨ CREATE | New how-to guide |

---

## 3. Management Portal Section

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `management-portal/index.md` | `setup-guides/management-ui/management-ui.md` | 🔄 MOVE | Portal overview |
| `management-portal/quick-start/portal-tour.md` | - | ✨ CREATE | New tour guide |
| `management-portal/quick-start/creating-first-agent.md` | - | ✨ CREATE | New quick start |
| `management-portal/how-to-guides/agents/create-new-agent.md` | `setup-guides/management-ui/management-ui.md` | 📋 COPY | Extract agent creation section |
| `management-portal/how-to-guides/agents/create-model-agnostic-agent-claude.md` | `how-to-guides/create-model-agnostic-agent-claude.md` | 🔄 MOVE | Full file move |
| `management-portal/how-to-guides/agents/create-model-agnostic-agent-gpt4o.md` | `how-to-guides/create-model-agnostic-agent-gpt4o.md` | 🔄 MOVE | Full file move |
| `management-portal/how-to-guides/agents/all-agents.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/agents/my-agents.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/agents/prompts.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/data-sources.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/data-pipelines/creating-data-pipelines.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/data-pipelines/invoking-data-pipelines.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/data-pipelines/monitoring-data-pipelines.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/data-pipeline-runs.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/knowledge-sources/sharepoint-online.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/knowledge-sources/azure-data-lake.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/knowledge-sources/private-storage.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/knowledge-sources/knowledge-graph-integration.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/data/knowledge-sources/image-description.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/models-endpoints/ai-models.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/models-endpoints/api-endpoints.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/security/instance-access-control.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/fllm-platform/branding.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/fllm-platform/configuration.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/fllm-platform/deployment-information.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/managing-plugins.md` | - | ✨ CREATE | New guide |
| `management-portal/how-to-guides/configuring-quotas.md` | - | ✨ CREATE | New guide |
| `management-portal/reference/concepts/agents-workflows.md` | `setup-guides/agents/agents_workflows.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/concepts/agent-access-tokens.md` | `setup-guides/agents/Agent_AccessToken.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/concepts/prompts-resources.md` | `setup-guides/agents/prompt-resource.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/concepts/knowledge-management-agent.md` | `setup-guides/agents/knowledge-management-agent.md` | 🗄️ ARCHIVE | Marked OBSOLETE |
| `management-portal/reference/concepts/resource-management.md` | `setup-guides/exposed-apis/resource-management/resource-management.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/concepts/data-pipelines.md` | `concepts/data-pipeline/data-pipeline.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/concepts/plugins-packages.md` | `concepts/plugin/plugin.md`, `concepts/plugin/plugin-package.md` | 🔄 MOVE | Merge both files |
| `management-portal/reference/concepts/quotas.md` | `concepts/quota/*.md` | 🔄 MOVE | Merge quota files |
| `management-portal/reference/branding/index.md` | `setup-guides/branding/index.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/branding/using-app-configuration.md` | `setup-guides/branding/branding-app-configuration.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/branding/using-management-portal.md` | `setup-guides/branding/branding-management-portal.md` | 🔄 MOVE | Full file move |
| `management-portal/reference/branding/using-rest-api.md` | - | ✨ CREATE | New stub |
| `management-portal/reference/configuration-reference.md` | - | ✨ CREATE | Generate from code constants |
| `management-portal/reference/permissions-roles.md` | - | ✨ CREATE | Generate from code constants |

---

## 4. APIs & SDKs Section

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `apis-sdks/apis/core-api/index.md` | `setup-guides/exposed-apis/core-api.md` | 🔄 MOVE | Full file move |
| `apis-sdks/apis/core-api/finding-core-api-url.md` | `setup-guides/quickstart.md` | 📋 COPY | Extract URL finding section |
| `apis-sdks/apis/core-api/directly-calling-core-api.md` | `development/calling-apis/directly-calling-core-api.md` | 🔄 MOVE | Full file move |
| `apis-sdks/apis/core-api/standard-deployment-local-api-access.md` | `development/calling-apis/standard-deployment-local-api-access.md` | 🔄 MOVE | Full file move |
| `apis-sdks/apis/core-api/api-reference.md` | `api/index.md` | 🔄 MOVE | Full file move |
| `apis-sdks/apis/management-api/index.md` | `setup-guides/exposed-apis/management-api.md` | 🔄 MOVE | Full file move |
| `apis-sdks/apis/management-api/resource-providers-overview.md` | - | ✨ CREATE | Generate from code |
| `apis-sdks/apis/management-api/directly-calling-management-api.md` | `development/calling-apis/directly-calling-management-api.md` | 🔄 MOVE | Full file move |
| `apis-sdks/apis/management-api/api-reference.md` | - | ✨ CREATE | New stub |
| `apis-sdks/apis/management-api/data-pipelines.md` | - | ✨ CREATE | New stub |
| `apis-sdks/sdks/dotnet/index.md` | `api/dotnet/index.md` | 🔄 MOVE | Full file move |
| `apis-sdks/sdks/python/index.md` | `api/python/index.md` | 🔄 MOVE | Full file move |

---

## 5. Platform Operations Section

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `platform-operations/deployment/index.md` | `deployment/index.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/deployment-quick-start.md` | `deployment/deployment-quick-start.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/deployment-standard.md` | `deployment/deployment-standard.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/deployment-configuration.md` | `deployment/deployment-configuration.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/app-configuration-values.md` | `deployment/app-configuration-values.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/azure-resource-providers-requirements.md` | `deployment/azure-resource-providers-requirements.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/custom-domains.md` | `deployment/custom-domains.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/soft-delete.md` | `deployment/soft-delete.md` | 🔄 MOVE | Full file move |
| `platform-operations/deployment/standard-manifest.md` | `deployment/standard/manifest.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/platform-security.md` | `operations/security.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/authentication-authorization/index.md` | `deployment/authentication-authorization/index.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/authentication-authorization/pre-deployment/core-authentication-setup.md` | `deployment/authentication-authorization/core-authentication-setup-entra.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/authentication-authorization/pre-deployment/management-authentication-setup.md` | `deployment/authentication-authorization/management-authentication-setup-entra.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/authentication-authorization/pre-deployment/authorization-setup.md` | `deployment/authentication-authorization/authorization-setup-entra.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/authentication-authorization/post-deployment/core-authentication-post.md` | `deployment/authentication-authorization/post-core-deployment.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/authentication-authorization/post-deployment/management-authentication-post.md` | `deployment/authentication-authorization/post-management-deployment.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/authentication-authorization/post-deployment/authorization-post.md` | `deployment/authentication-authorization/post-authorization-deployment.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/role-based-access-control/index.md` | `role-based-access-control/index.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/role-based-access-control/role-definitions.md` | `role-based-access-control/role-definitions.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/role-based-access-control/role-assignments.md` | `role-based-access-control/role-assignments.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/role-based-access-control/scope.md` | `role-based-access-control/scope.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/role-based-access-control/role-management.md` | `role-based-access-control/role-management.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/role-based-access-control/agent-role-assignments.md` | `role-based-access-control/agent-role-assignments.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/configure-access-control-services.md` | `deployment/configure-access-control-for-services.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/graph-api-permissions.md` | `operations/graph-api-permissions.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/network-security-groups.md` | `operations/network-security-groups.md` | 🔄 MOVE | Full file move |
| `platform-operations/security-permissions/vulnerabilities.md` | `operations/vulnerabilities.md` | 🔄 MOVE | Full file move |
| `platform-operations/monitoring-troubleshooting/logs.md` | `operations/logs.md` | 🔄 MOVE | Full file move |
| `platform-operations/monitoring-troubleshooting/troubleshooting.md` | `operations/troubleshooting.md` | 🔄 MOVE | Full file move |
| `platform-operations/how-to-guides/updating-container-versions.md` | `operations/update.md` | 🔄 MOVE | Full file move |
| `platform-operations/how-to-guides/backups.md` | `operations/backups.md` | 🔄 MOVE | Full file move |
| `platform-operations/how-to-guides/purge-conversations.md` | `operations/purge-conversations.md` | 🔄 MOVE | Full file move |
| `platform-operations/how-to-guides/creating-release-notes.md` | `operations/release-notes.md` | 🔄 MOVE | Full file move |

---

## 6. Development Section

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `development/index.md` | `development/index.md` | 🔄 MOVE | Full file move |
| `development/development-approach.md` | `development/development-approach.md` | 🔄 MOVE | Full file move |
| `development/development-local.md` | `development/development-local.md` | 🔄 MOVE | Full file move |
| `development/contributing/index.md` | `development/contributing/index.md` | 🔄 MOVE | Full file move |
| `development/contributing/git-workflow.md` | `development/contributing/git-workflow.md` | 🔄 MOVE | Full file move |
| `development/contributing/style-guide.md` | `development/contributing/style-guide.md` | 🔄 MOVE | Full file move |
| `development/contributing/bug-report-reproduction.md` | `development/contributing/repro.md` | 🔄 MOVE | Full file move |

---

## 7. Reference Section

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `reference/agent-schemas.md` | `schema.md` | 🔄 MOVE | Full file move |
| `reference/release-notes/index.md` | - | ✨ CREATE | New index for release notes |
| `reference/release-notes/breaking-changes.md` | `release-notes/breaking-changes.md` | 🔄 MOVE | Full file move |
| `reference/release-notes/release_notes_0.9.7.md` | `release-notes/release_notes_0.9.7.md` | 🔄 MOVE | Full file move |
| `reference/documentation-generation.md` | `documentation-generation.md` | 🔄 MOVE | Full file move |

---

## 8. Archive Section (Obsolete Content)

| New Location | Source | Action | Notes |
|--------------|--------|--------|-------|
| `archive/vectorization/index.md` | `setup-guides/vectorization/index.md` | 🗄️ ARCHIVE | Per outline: DROP |
| `archive/vectorization/vectorization-concepts.md` | `setup-guides/vectorization/vectorization-concepts.md` | 🗄️ ARCHIVE | Per outline: DROP |
| `archive/vectorization/vectorization-configuration.md` | `setup-guides/vectorization/vectorization-configuration.md` | 🗄️ ARCHIVE | Per outline: DROP |
| `archive/vectorization/vectorization-profiles.md` | `setup-guides/vectorization/vectorization-profiles.md` | 🗄️ ARCHIVE | Per outline: DROP |
| `archive/vectorization/vectorization-triggering.md` | `setup-guides/vectorization/vectorization-triggering.md` | 🗄️ ARCHIVE | Per outline: DROP |
| `archive/vectorization/vectorization-monitoring-troubleshooting.md` | `setup-guides/vectorization/vectorization-monitoring-troubleshooting.md` | 🗄️ ARCHIVE | Per outline: DROP |
| `archive/directly-calling-vectorization-api.md` | `development/calling-apis/directly-calling-vectorization-api.md` | 🗄️ ARCHIVE | Per outline: OBSOLETE |

---

## Content Requiring Additional Review

The following items need additional attention during integration:

### Files with Partial Content Extraction

| Source File | Content to Extract | Destination |
|-------------|-------------------|-------------|
| `index.md` | "Why FoundationaLLM?" section | `overview/why-foundationallm.md` |
| `concepts/index.md` | Architecture overview/mindmap | `overview/architecture-concepts.md` |
| `setup-guides/quickstart.md` | "Find your Core API URL" section | `apis-sdks/apis/core-api/finding-core-api-url.md` |
| `setup-guides/management-ui/management-ui.md` | Agent creation steps | `management-portal/how-to-guides/agents/create-new-agent.md` |

### Files to Generate from Code

| Destination | Source Code Location | Notes |
|-------------|---------------------|-------|
| `management-portal/reference/configuration-reference.md` | `src/dotnet/Common/Constants/Data` | Generate from code constants |
| `management-portal/reference/permissions-roles.md` | `src/dotnet/Common/Constants/Data/AuthorizableActions.json`, `RoleDefinitions.json` | Generate from code constants |
| `apis-sdks/apis/management-api/resource-providers-overview.md` | `src/dotnet/Common/Constants/ResourceProviders` | Generate from metadata files |

### Internal Links to Update

After reorganization, the following types of links will need updating:
- All relative links between documentation files
- References to media files (may need to move/consolidate media folders)
- TOC file (`toc.yml`) needs complete rewrite
- Links in `docfx.json` configuration

### Media Files

The following media folders exist and may need consolidation:
- `docs/media/` (root media)
- `docs/setup-guides/media/`
- `docs/setup-guides/agents/media/`
- `docs/setup-guides/vectorization/media/`

**Recommendation:** Create a centralized `docs/media/` structure organized by section.

---

## Implementation Phases

### Phase 1: Create New Folder Structure
Create all new directories without moving files.

### Phase 2: Move Existing Files
Move files that map 1:1 to new locations.

### Phase 3: Extract Partial Content
Create new files from partial content extraction.

### Phase 4: Create Stub Files
Create placeholder files for new content.

### Phase 5: Archive Obsolete Content
Move vectorization and obsolete content to archive.

### Phase 6: Update Links and TOC
Update all internal links and rebuild TOC.

### Phase 7: Media Consolidation
Organize and consolidate media files.

### Phase 8: Validation
Verify all links, test docfx build.

---

## Change Tracking Summary

### Existing Files Affected

| Original Location | Status | New Location |
|-------------------|--------|--------------|
| `index.md` | MOVE + EXTRACT | `overview/index.md` |
| `concepts/index.md` | KEEP + EXTRACT | (stays, content extracted to overview) |
| `concepts/data-pipeline/data-pipeline.md` | MOVE | `management-portal/reference/concepts/data-pipelines.md` |
| `concepts/plugin/plugin.md` | MOVE | `management-portal/reference/concepts/plugins-packages.md` |
| `concepts/plugin/plugin-package.md` | MERGE | `management-portal/reference/concepts/plugins-packages.md` |
| `concepts/prompt/prompt-variable.md` | KEEP | (already in concepts) |
| `concepts/quota/quota-definition.md` | MERGE | `management-portal/reference/concepts/quotas.md` |
| `concepts/quota/agent-request-rate.md` | MERGE | `management-portal/reference/concepts/quotas.md` |
| `concepts/quota/api-raw-request-rate.md` | MERGE | `management-portal/reference/concepts/quotas.md` |
| `deployment/index.md` | MOVE | `platform-operations/deployment/index.md` |
| `deployment/deployment-quick-start.md` | MOVE | `platform-operations/deployment/deployment-quick-start.md` |
| `deployment/deployment-standard.md` | MOVE | `platform-operations/deployment/deployment-standard.md` |
| `deployment/deployment-configuration.md` | MOVE | `platform-operations/deployment/deployment-configuration.md` |
| `deployment/app-configuration-values.md` | MOVE | `platform-operations/deployment/app-configuration-values.md` |
| `deployment/azure-resource-providers-requirements.md` | MOVE | `platform-operations/deployment/azure-resource-providers-requirements.md` |
| `deployment/configure-access-control-for-services.md` | MOVE | `platform-operations/security-permissions/configure-access-control-services.md` |
| `deployment/custom-domains.md` | MOVE | `platform-operations/deployment/custom-domains.md` |
| `deployment/soft-delete.md` | MOVE | `platform-operations/deployment/soft-delete.md` |
| `deployment/standard/manifest.md` | MOVE | `platform-operations/deployment/standard-manifest.md` |
| `deployment/authentication-authorization/index.md` | MOVE | `platform-operations/security-permissions/authentication-authorization/index.md` |
| `deployment/authentication-authorization/core-authentication-setup-entra.md` | MOVE | `platform-operations/security-permissions/authentication-authorization/pre-deployment/core-authentication-setup.md` |
| `deployment/authentication-authorization/management-authentication-setup-entra.md` | MOVE | `platform-operations/security-permissions/authentication-authorization/pre-deployment/management-authentication-setup.md` |
| `deployment/authentication-authorization/authorization-setup-entra.md` | MOVE | `platform-operations/security-permissions/authentication-authorization/pre-deployment/authorization-setup.md` |
| `deployment/authentication-authorization/post-core-deployment.md` | MOVE | `platform-operations/security-permissions/authentication-authorization/post-deployment/core-authentication-post.md` |
| `deployment/authentication-authorization/post-management-deployment.md` | MOVE | `platform-operations/security-permissions/authentication-authorization/post-deployment/management-authentication-post.md` |
| `deployment/authentication-authorization/post-authorization-deployment.md` | MOVE | `platform-operations/security-permissions/authentication-authorization/post-deployment/authorization-post.md` |
| `deployment/authentication-authorization/pre-requisites.md` | REVIEW | May merge into index |
| `development/index.md` | MOVE | `development/index.md` |
| `development/development-approach.md` | MOVE | `development/development-approach.md` |
| `development/development-local.md` | MOVE | `development/development-local.md` |
| `development/calling-apis/index.md` | REVIEW | Content may be distributed |
| `development/calling-apis/directly-calling-core-api.md` | MOVE | `apis-sdks/apis/core-api/directly-calling-core-api.md` |
| `development/calling-apis/directly-calling-management-api.md` | MOVE | `apis-sdks/apis/management-api/directly-calling-management-api.md` |
| `development/calling-apis/standard-deployment-local-api-access.md` | MOVE | `apis-sdks/apis/core-api/standard-deployment-local-api-access.md` |
| `development/contributing/index.md` | MOVE | `development/contributing/index.md` |
| `development/contributing/git-workflow.md` | MOVE | `development/contributing/git-workflow.md` |
| `development/contributing/repro.md` | MOVE | `development/contributing/bug-report-reproduction.md` |
| `development/contributing/style-guide.md` | MOVE | `development/contributing/style-guide.md` |
| `how-to-guides/create-model-agnostic-agent-claude.md` | MOVE | `management-portal/how-to-guides/agents/create-model-agnostic-agent-claude.md` |
| `how-to-guides/create-model-agnostic-agent-gpt4o.md` | MOVE | `management-portal/how-to-guides/agents/create-model-agnostic-agent-gpt4o.md` |
| `operations/backups.md` | MOVE | `platform-operations/how-to-guides/backups.md` |
| `operations/graph-api-permissions.md` | MOVE | `platform-operations/security-permissions/graph-api-permissions.md` |
| `operations/index.md` | REVIEW | Content may be distributed |
| `operations/logs.md` | MOVE | `platform-operations/monitoring-troubleshooting/logs.md` |
| `operations/network-security-groups.md` | MOVE | `platform-operations/security-permissions/network-security-groups.md` |
| `operations/purge-conversations.md` | MOVE | `platform-operations/how-to-guides/purge-conversations.md` |
| `operations/release-notes.md` | MOVE | `platform-operations/how-to-guides/creating-release-notes.md` |
| `operations/security.md` | MOVE | `platform-operations/security-permissions/platform-security.md` |
| `operations/troubleshooting.md` | MOVE | `platform-operations/monitoring-troubleshooting/troubleshooting.md` |
| `operations/update.md` | MOVE | `platform-operations/how-to-guides/updating-container-versions.md` |
| `operations/vulnerabilities.md` | MOVE | `platform-operations/security-permissions/vulnerabilities.md` |
| `role-based-access-control/index.md` | MOVE | `platform-operations/security-permissions/role-based-access-control/index.md` |
| `role-based-access-control/agent-role-assignments.md` | MOVE | `platform-operations/security-permissions/role-based-access-control/agent-role-assignments.md` |
| `role-based-access-control/role-assignments.md` | MOVE | `platform-operations/security-permissions/role-based-access-control/role-assignments.md` |
| `role-based-access-control/role-definitions.md` | MOVE | `platform-operations/security-permissions/role-based-access-control/role-definitions.md` |
| `role-based-access-control/role-management.md` | MOVE | `platform-operations/security-permissions/role-based-access-control/role-management.md` |
| `role-based-access-control/scope.md` | MOVE | `platform-operations/security-permissions/role-based-access-control/scope.md` |
| `setup-guides/index.md` | DEPRECATE | Content distributed to new sections |
| `setup-guides/quickstart.md` | MOVE + EXTRACT | `chat-user-portal/quick-start/quickstart.md` |
| `setup-guides/agents/index.md` | REVIEW | Content may be distributed |
| `setup-guides/agents/Agent_AccessToken.md` | MOVE | `management-portal/reference/concepts/agent-access-tokens.md` |
| `setup-guides/agents/agents_workflows.md` | MOVE | `management-portal/reference/concepts/agents-workflows.md` |
| `setup-guides/agents/knowledge-management-agent.md` | ARCHIVE | `archive/knowledge-management-agent.md` |
| `setup-guides/agents/prompt-resource.md` | MOVE | `management-portal/reference/concepts/prompts-resources.md` |
| `setup-guides/branding/index.md` | MOVE | `management-portal/reference/branding/index.md` |
| `setup-guides/branding/branding-app-configuration.md` | MOVE | `management-portal/reference/branding/using-app-configuration.md` |
| `setup-guides/branding/branding-management-portal.md` | MOVE | `management-portal/reference/branding/using-management-portal.md` |
| `setup-guides/exposed-apis/index.md` | REVIEW | Content distributed |
| `setup-guides/exposed-apis/core-api.md` | MOVE | `apis-sdks/apis/core-api/index.md` |
| `setup-guides/exposed-apis/management-api.md` | MOVE | `apis-sdks/apis/management-api/index.md` |
| `setup-guides/exposed-apis/resource-management/resource-management.md` | MOVE | `management-portal/reference/concepts/resource-management.md` |
| `setup-guides/management-ui/management-ui.md` | MOVE + EXTRACT | `management-portal/index.md` |
| `setup-guides/vectorization/index.md` | ARCHIVE | `archive/vectorization/index.md` |
| `setup-guides/vectorization/vectorization-concepts.md` | ARCHIVE | `archive/vectorization/vectorization-concepts.md` |
| `setup-guides/vectorization/vectorization-configuration.md` | ARCHIVE | `archive/vectorization/vectorization-configuration.md` |
| `setup-guides/vectorization/vectorization-monitoring-troubleshooting.md` | ARCHIVE | `archive/vectorization/vectorization-monitoring-troubleshooting.md` |
| `setup-guides/vectorization/vectorization-profiles.md` | ARCHIVE | `archive/vectorization/vectorization-profiles.md` |
| `setup-guides/vectorization/vectorization-triggering.md` | ARCHIVE | `archive/vectorization/vectorization-triggering.md` |
| `api/index.md` | MOVE | `apis-sdks/apis/core-api/api-reference.md` |
| `api/dotnet/index.md` | MOVE | `apis-sdks/sdks/dotnet/index.md` |
| `api/python/index.md` | MOVE | `apis-sdks/sdks/python/index.md` |
| `documentation-generation.md` | MOVE | `reference/documentation-generation.md` |
| `schema.md` | MOVE | `reference/agent-schemas.md` |
| `release-notes/breaking-changes.md` | MOVE | `reference/release-notes/breaking-changes.md` |
| `release-notes/release_notes_0.9.7.md` | MOVE | `reference/release-notes/release_notes_0.9.7.md` |
| `toc.yml` | REWRITE | Complete rewrite needed |
| `docfx.json` | UPDATE | Update paths |

---

## Files NOT Affected (Keep in Place)

| File | Reason |
|------|--------|
| `docfx.json` | Configuration file (update paths only) |
| `.gitignore` | Git configuration |
| `.ignore` | Search ignore |
| `concepts/prompt/prompt-variable.md` | Referenced by concepts index |

---

## Next Steps

1. **Review this plan** with stakeholders
2. **Prioritize** which sections to implement first
3. **Create stubs** for new content pages
4. **Execute moves** in logical order (start with leaf nodes)
5. **Update links** iteratively
6. **Test build** with docfx after each major change
7. **Finalize TOC** once structure is stable
