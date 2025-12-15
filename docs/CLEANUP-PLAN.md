# Documentation Cleanup Plan

This document outlines the plan to clean up legacy folders that remain after the documentation reorganization.

## Overview

After the reorganization documented in `REORGANIZATION-PLAN.md`, content has been moved to the new structure. However, the original folders remain and should be moved to the `archive/` folder to maintain a clean documentation structure.

## Current State

### New Structure (KEEP)

These folders are part of the new documentation structure and should remain:

| Folder | Purpose |
|--------|---------|
| `apis-sdks/` | API and SDK documentation |
| `archive/` | Archived/obsolete content |
| `chat-user-portal/` | End-user documentation |
| `development/` | Developer contribution guides |
| `management-portal/` | Management Portal documentation |
| `overview/` | Platform overview |
| `platform-operations/` | Deployment and operations |
| `reference/` | Reference documentation |

### Legacy Folders (ARCHIVE)

These folders contain the original content that has been reorganized:

| Folder | Files | Destination |
|--------|-------|-------------|
| `api/` | 4 files | Content moved to `apis-sdks/` |
| `concepts/` | 8 files | Content distributed to `management-portal/reference/` |
| `deployment/` | 17 files | Content moved to `platform-operations/deployment/` |
| `how-to-guides/` | 2 files | Content moved to `management-portal/how-to-guides/` |
| `operations/` | 11 files | Content moved to `platform-operations/` |
| `release-notes/` | 2 files | Content moved to `reference/release-notes/` |
| `role-based-access-control/` | 6 files | Content moved to `platform-operations/security-permissions/` |
| `setup-guides/` | 18 files | Content distributed across sections |

## Cleanup Actions

### Phase 1: Archive Legacy Folders

Move each legacy folder to `archive/legacy-[foldername]/`:

```bash
# Create archive subdirectories
mkdir -p docs/archive/legacy-api
mkdir -p docs/archive/legacy-concepts
mkdir -p docs/archive/legacy-deployment
mkdir -p docs/archive/legacy-how-to-guides
mkdir -p docs/archive/legacy-operations
mkdir -p docs/archive/legacy-release-notes
mkdir -p docs/archive/legacy-role-based-access-control
mkdir -p docs/archive/legacy-setup-guides
```

### Phase 2: Move Folders

| Source | Destination |
|--------|-------------|
| `docs/api/*` | `docs/archive/legacy-api/` |
| `docs/concepts/*` | `docs/archive/legacy-concepts/` |
| `docs/deployment/*` | `docs/archive/legacy-deployment/` |
| `docs/how-to-guides/*` | `docs/archive/legacy-how-to-guides/` |
| `docs/operations/*` | `docs/archive/legacy-operations/` |
| `docs/release-notes/*` | `docs/archive/legacy-release-notes/` |
| `docs/role-based-access-control/*` | `docs/archive/legacy-role-based-access-control/` |
| `docs/setup-guides/*` | `docs/archive/legacy-setup-guides/` |

### Phase 3: Remove Empty Directories

After moving content, remove the empty legacy directories:

```bash
rmdir docs/api
rmdir docs/concepts
rmdir docs/deployment
rmdir docs/how-to-guides
rmdir docs/operations
rmdir docs/release-notes
rmdir docs/role-based-access-control
rmdir docs/setup-guides
```

## Files to Keep in Root

The following files should remain in the `docs/` root:

| File | Purpose |
|------|---------|
| `docfx.json` | Documentation build configuration |
| `toc.yml` | Table of contents |
| `outline.md` | Documentation structure reference |
| `schema.md` | Schema reference (may move to reference/) |
| `REORGANIZATION-PLAN.md` | Reorganization tracking |
| `CONTENT-TRACKING.md` | Content tracking |
| `CLEANUP-PLAN.md` | This document |

## Final Folder Structure

After cleanup, `docs/` should contain:

```
docs/
├── apis-sdks/           # API and SDK documentation
├── archive/             # Archived content
│   ├── legacy-api/
│   ├── legacy-concepts/
│   ├── legacy-deployment/
│   ├── legacy-how-to-guides/
│   ├── legacy-operations/
│   ├── legacy-release-notes/
│   ├── legacy-role-based-access-control/
│   ├── legacy-setup-guides/
│   ├── knowledge-management-agent.md
│   ├── README.md
│   └── vectorization/
├── chat-user-portal/    # End-user documentation
├── development/         # Developer guides
├── management-portal/   # Management Portal documentation
├── overview/            # Platform overview
├── platform-operations/ # Deployment and operations
├── reference/           # Reference documentation
├── docfx.json          # Build configuration
├── toc.yml             # Table of contents
├── outline.md          # Structure reference
├── schema.md           # Schema reference
├── REORGANIZATION-PLAN.md
├── CONTENT-TRACKING.md
└── CLEANUP-PLAN.md
```

## Archive README Update

Update `archive/README.md` to document the archived content:

```markdown
# Archived Documentation

This folder contains documentation that has been superseded by the reorganized 
documentation structure.

## Contents

| Folder | Original Location | Archived Date | Reason |
|--------|-------------------|---------------|--------|
| `vectorization/` | `setup-guides/vectorization/` | 2024-xx-xx | Feature deprecated |
| `legacy-api/` | `api/` | 2024-xx-xx | Moved to `apis-sdks/` |
| `legacy-concepts/` | `concepts/` | 2024-xx-xx | Distributed to new sections |
| `legacy-deployment/` | `deployment/` | 2024-xx-xx | Moved to `platform-operations/` |
| `legacy-how-to-guides/` | `how-to-guides/` | 2024-xx-xx | Moved to `management-portal/` |
| `legacy-operations/` | `operations/` | 2024-xx-xx | Moved to `platform-operations/` |
| `legacy-release-notes/` | `release-notes/` | 2024-xx-xx | Moved to `reference/release-notes/` |
| `legacy-role-based-access-control/` | `role-based-access-control/` | 2024-xx-xx | Moved to `platform-operations/security-permissions/` |
| `legacy-setup-guides/` | `setup-guides/` | 2024-xx-xx | Distributed to new sections |

## Note

This content is kept for historical reference. Do not link to these files 
from the main documentation. If you need content from these files, refer 
to the corresponding files in the new documentation structure.
```

## Execution Commands

Execute the cleanup in order:

```bash
# Step 1: Create archive directories
cd /workspace/docs
mkdir -p archive/legacy-api
mkdir -p archive/legacy-concepts
mkdir -p archive/legacy-deployment
mkdir -p archive/legacy-how-to-guides
mkdir -p archive/legacy-operations
mkdir -p archive/legacy-release-notes
mkdir -p archive/legacy-role-based-access-control
mkdir -p archive/legacy-setup-guides

# Step 2: Move content (preserving structure)
mv api/* archive/legacy-api/
mv concepts/* archive/legacy-concepts/
mv deployment/* archive/legacy-deployment/
mv how-to-guides/* archive/legacy-how-to-guides/
mv operations/* archive/legacy-operations/
mv release-notes/* archive/legacy-release-notes/
mv role-based-access-control/* archive/legacy-role-based-access-control/
mv setup-guides/* archive/legacy-setup-guides/

# Step 3: Remove empty directories
rmdir api
rmdir concepts
rmdir deployment
rmdir how-to-guides
rmdir operations
rmdir release-notes
rmdir role-based-access-control
rmdir setup-guides
```

## Post-Cleanup Tasks

1. **Update `toc.yml`** - Ensure TOC only references new structure
2. **Update `docfx.json`** - Update any path references
3. **Verify build** - Run docfx to ensure documentation builds
4. **Update any external links** - If documentation is published, update bookmarks

## Notes

- The `development/` folder is part of the new structure per `outline.md` and should NOT be archived
- The `reference/` folder is part of the new structure and should NOT be archived
- Media files within archived folders will be preserved in the archive
- `.ignore` and `.gitignore` files will be moved with their folders
