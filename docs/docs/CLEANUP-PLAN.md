# Documentation Cleanup Plan

> **Status:** ✅ EXECUTED on 2024-12-14
> **Update:** Removed redundant `reference/` folder on 2024-12-14 (content duplicated root-level files)

This document outlines the plan to clean up legacy folders that remain after the documentation reorganization.

## Overview

After the reorganization documented in `REORGANIZATION-PLAN.md`, content has been moved to the new structure. However, the original folders remain and should be cleaned up.

## Pre-Cleanup Actions

Before archiving, the following content needs to be migrated to the new structure:

### 1. Concepts → Management Portal

The `concepts/` folder has two files that need to be moved to `management-portal/reference/concepts/`:

| Source | Destination | Action |
|--------|-------------|--------|
| `concepts/index.md` | `management-portal/reference/concepts/index.md` | MOVE (glossary/overview) |
| `concepts/prompt/prompt-variable.md` | `management-portal/reference/concepts/prompt-variables.md` | MOVE |

The other `concepts/` content has already been migrated:
- `concepts/data-pipeline/data-pipeline.md` → ✅ `management-portal/reference/concepts/data-pipelines.md`
- `concepts/plugin/*.md` → ✅ `management-portal/reference/concepts/plugins-packages.md`
- `concepts/quota/*.md` → ✅ `management-portal/reference/concepts/quotas.md`

### 2. Release Notes - Keep in Place

The `release-notes/` folder should remain at the root level as part of the new documentation structure. Update `outline.md` and `REORGANIZATION-PLAN.md` to reflect this.

**Rationale:** Release notes are a top-level documentation concern and should be easily discoverable at the root level alongside other primary sections.

## Verification Status

### ✅ deployment/ → platform-operations/deployment/

| Original File | New Location | Status |
|---------------|--------------|--------|
| `deployment/index.md` | `platform-operations/deployment/index.md` | ✅ Migrated |
| `deployment/deployment-quick-start.md` | `platform-operations/deployment/deployment-quick-start.md` | ✅ Migrated |
| `deployment/deployment-standard.md` | `platform-operations/deployment/deployment-standard.md` | ✅ Migrated |
| `deployment/deployment-configuration.md` | `platform-operations/deployment/deployment-configuration.md` | ✅ Migrated |
| `deployment/app-configuration-values.md` | `platform-operations/deployment/app-configuration-values.md` | ✅ Migrated |
| `deployment/azure-resource-providers-requirements.md` | `platform-operations/deployment/azure-resource-providers-requirements.md` | ✅ Migrated |
| `deployment/custom-domains.md` | `platform-operations/deployment/custom-domains.md` | ✅ Migrated |
| `deployment/soft-delete.md` | `platform-operations/deployment/soft-delete.md` | ✅ Migrated |
| `deployment/standard/manifest.md` | `platform-operations/deployment/standard-manifest.md` | ✅ Migrated |
| `deployment/configure-access-control-for-services.md` | `platform-operations/security-permissions/configure-access-control-services.md` | ✅ Migrated |
| `deployment/authentication-authorization/*` | `platform-operations/security-permissions/authentication-authorization/*` | ✅ Migrated |

### ✅ role-based-access-control/ → platform-operations/security-permissions/role-based-access-control/

| Original File | New Location | Status |
|---------------|--------------|--------|
| `role-based-access-control/index.md` | `platform-operations/security-permissions/role-based-access-control/index.md` | ✅ Migrated |
| `role-based-access-control/role-definitions.md` | `platform-operations/security-permissions/role-based-access-control/role-definitions.md` | ✅ Migrated |
| `role-based-access-control/role-assignments.md` | `platform-operations/security-permissions/role-based-access-control/role-assignments.md` | ✅ Migrated |
| `role-based-access-control/scope.md` | `platform-operations/security-permissions/role-based-access-control/scope.md` | ✅ Migrated |
| `role-based-access-control/role-management.md` | `platform-operations/security-permissions/role-based-access-control/role-management.md` | ✅ Migrated |
| `role-based-access-control/agent-role-assignments.md` | `platform-operations/security-permissions/role-based-access-control/agent-role-assignments.md` | ✅ Migrated |

### ✅ setup-guides/ → Various Locations

| Original Location | New Location | Status |
|-------------------|--------------|--------|
| `setup-guides/agents/agents_workflows.md` | `management-portal/reference/concepts/agents-workflows.md` | ✅ Migrated |
| `setup-guides/agents/Agent_AccessToken.md` | `management-portal/reference/concepts/agent-access-tokens.md` | ✅ Migrated |
| `setup-guides/agents/prompt-resource.md` | `management-portal/reference/concepts/prompts-resources.md` | ✅ Migrated |
| `setup-guides/agents/knowledge-management-agent.md` | `archive/knowledge-management-agent.md` | ✅ Archived (obsolete) |
| `setup-guides/branding/*` | `management-portal/reference/branding/*` | ✅ Migrated |
| `setup-guides/exposed-apis/core-api.md` | `apis-sdks/apis/core-api/index.md` | ✅ Migrated |
| `setup-guides/exposed-apis/management-api.md` | `apis-sdks/apis/management-api/index.md` | ✅ Migrated |
| `setup-guides/exposed-apis/resource-management/*` | `management-portal/reference/concepts/resource-management.md` | ✅ Migrated |
| `setup-guides/management-ui/management-ui.md` | `management-portal/index.md` | ✅ Migrated |
| `setup-guides/quickstart.md` | `chat-user-portal/quick-start/quickstart.md` | ✅ Migrated |
| `setup-guides/vectorization/*` | `archive/vectorization/*` | ✅ Archived (obsolete) |

### ✅ operations/ → platform-operations/

| Original File | New Location | Status |
|---------------|--------------|--------|
| `operations/backups.md` | `platform-operations/how-to-guides/backups.md` | ✅ Migrated |
| `operations/logs.md` | `platform-operations/monitoring-troubleshooting/logs.md` | ✅ Migrated |
| `operations/troubleshooting.md` | `platform-operations/monitoring-troubleshooting/troubleshooting.md` | ✅ Migrated |
| `operations/security.md` | `platform-operations/security-permissions/platform-security.md` | ✅ Migrated |
| `operations/graph-api-permissions.md` | `platform-operations/security-permissions/graph-api-permissions.md` | ✅ Migrated |
| `operations/network-security-groups.md` | `platform-operations/security-permissions/network-security-groups.md` | ✅ Migrated |
| `operations/vulnerabilities.md` | `platform-operations/security-permissions/vulnerabilities.md` | ✅ Migrated |
| `operations/update.md` | `platform-operations/how-to-guides/updating-container-versions.md` | ✅ Migrated |
| `operations/purge-conversations.md` | `platform-operations/how-to-guides/purge-conversations.md` | ✅ Migrated |
| `operations/release-notes.md` | `platform-operations/how-to-guides/creating-release-notes.md` | ✅ Migrated |

### ✅ how-to-guides/ → management-portal/how-to-guides/

| Original File | New Location | Status |
|---------------|--------------|--------|
| `how-to-guides/create-model-agnostic-agent-claude.md` | `management-portal/how-to-guides/agents/create-model-agnostic-agent-claude.md` | ✅ Migrated |
| `how-to-guides/create-model-agnostic-agent-gpt4o.md` | `management-portal/how-to-guides/agents/create-model-agnostic-agent-gpt4o.md` | ✅ Migrated |

### ✅ api/ → apis-sdks/

| Original File | New Location | Status |
|---------------|--------------|--------|
| `api/index.md` | `apis-sdks/apis/core-api/api-reference.md` | ✅ Migrated |
| `api/dotnet/index.md` | `apis-sdks/sdks/dotnet/index.md` | ✅ Migrated |
| `api/python/index.md` | `apis-sdks/sdks/python/index.md` | ✅ Migrated |

## Execution Plan

### Phase 1: Migrate Remaining Concepts Content

```bash
# Move concepts/index.md to management-portal/reference/concepts/
mv docs/concepts/index.md docs/management-portal/reference/concepts/index.md

# Move prompt-variable.md
mv docs/concepts/prompt/prompt-variable.md docs/management-portal/reference/concepts/prompt-variables.md
```

### Phase 2: Archive Legacy Folders

Move legacy folders to `archive/` without "legacy-" prefix:

```bash
# Create archive subdirectories
mkdir -p docs/archive/api
mkdir -p docs/archive/concepts
mkdir -p docs/archive/deployment
mkdir -p docs/archive/how-to-guides
mkdir -p docs/archive/operations
mkdir -p docs/archive/role-based-access-control
mkdir -p docs/archive/setup-guides

# Move content to archive
mv docs/api/* docs/archive/api/
mv docs/concepts/* docs/archive/concepts/
mv docs/deployment/* docs/archive/deployment/
mv docs/how-to-guides/* docs/archive/how-to-guides/
mv docs/operations/* docs/archive/operations/
mv docs/role-based-access-control/* docs/archive/role-based-access-control/
mv docs/setup-guides/* docs/archive/setup-guides/

# Remove empty directories
rmdir docs/api
rmdir docs/concepts
rmdir docs/deployment
rmdir docs/how-to-guides
rmdir docs/operations
rmdir docs/role-based-access-control
rmdir docs/setup-guides
```

### Phase 3: Keep release-notes in Place

The `release-notes/` folder remains at the root level. No action needed.

## Final Folder Structure

After cleanup:

```
docs/
├── apis-sdks/               # API and SDK documentation
├── archive/                 # Archived content
│   ├── api/                 # Old API docs
│   ├── concepts/            # Old concepts docs
│   ├── deployment/          # Old deployment docs
│   ├── how-to-guides/       # Old how-to guides
│   ├── knowledge-management-agent.md
│   ├── operations/          # Old operations docs
│   ├── README.md
│   ├── role-based-access-control/  # Old RBAC docs
│   ├── setup-guides/        # Old setup guides
│   └── vectorization/       # Obsolete vectorization docs
├── chat-user-portal/        # End-user documentation
├── development/             # Developer guides
├── management-portal/       # Management Portal documentation
├── overview/                # Platform overview
├── platform-operations/     # Deployment and operations
├── release-notes/           # Release notes (kept at root)
│   ├── breaking-changes.md
│   └── release_notes_0.9.7.md
├── docfx.json              # Build configuration
├── toc.yml                 # Table of contents
├── outline.md              # Structure reference
├── schema.md               # Schema reference
├── REORGANIZATION-PLAN.md
├── CONTENT-TRACKING.md
└── CLEANUP-PLAN.md
```

## Update to Documentation Structure

### Addition to outline.md

Add `release-notes/` as a top-level section:

```
docs/
├── ...existing sections...
├── release-notes/           # Release notes
│   ├── breaking-changes.md
│   └── [version-specific files]
```

### Update archive/README.md

```markdown
# Archived Documentation

This folder contains documentation that has been superseded by the reorganized 
documentation structure.

## Contents

| Folder | Original Purpose | Archived Date | Reason |
|--------|-----------------|---------------|--------|
| `api/` | API reference | 2024-12-14 | Moved to `apis-sdks/` |
| `concepts/` | Concept definitions | 2024-12-14 | Moved to `management-portal/reference/concepts/` |
| `deployment/` | Deployment guides | 2024-12-14 | Moved to `platform-operations/deployment/` |
| `how-to-guides/` | How-to guides | 2024-12-14 | Moved to `management-portal/how-to-guides/` |
| `operations/` | Operations guides | 2024-12-14 | Moved to `platform-operations/` |
| `role-based-access-control/` | RBAC documentation | 2024-12-14 | Moved to `platform-operations/security-permissions/` |
| `setup-guides/` | Setup guides | 2024-12-14 | Distributed to new sections |
| `vectorization/` | Vectorization docs | 2024-12-14 | Feature deprecated |
| `knowledge-management-agent.md` | Agent type docs | 2024-12-14 | Obsolete |

## Note

This content is kept for historical reference. Do not link to these files 
from the main documentation. For current documentation, refer to the 
corresponding files in the new documentation structure.
```

## Post-Cleanup Tasks

1. **Update `toc.yml`** - Ensure TOC includes `release-notes/` at root level
2. **Update `outline.md`** - Add `release-notes/` to the documented structure
3. **Update `docfx.json`** - Ensure paths are correct
4. **Verify build** - Run docfx to ensure documentation builds
5. **Update any external links** - If documentation is published, update bookmarks
