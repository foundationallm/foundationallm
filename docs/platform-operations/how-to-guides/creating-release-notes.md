# Creating Release Notes

This guide provides the process for creating release notes for FoundationaLLM updates.

## Overview

Release notes communicate changes to end-users, stakeholders, and operations teams. Well-structured notes help with:

- Understanding what's new or changed
- Planning upgrade timing
- Identifying breaking changes
- Tracking project evolution

## Release Notes Structure

### Standard Format

```markdown
# Release X.Y.Z

**Release Date:** YYYY-MM-DD

## Highlights
- Major feature or change summary

## New Features
- Feature 1 description (#issue-number)
- Feature 2 description (#issue-number)

## Enhancements
- Enhancement 1 description (#issue-number)

## Bug Fixes
- Fix 1 description (#issue-number)

## Breaking Changes
- Description of breaking change
- Migration steps

## Deprecations
- Feature being deprecated
- Removal timeline

## Security Updates
- Security-related fixes

## Known Issues
- Outstanding issues

## Upgrade Instructions
- Steps to upgrade
```

## Process

### 1. Define Release Scope

- Review the milestone or sprint scope
- Identify all changes included
- Confirm release version number

### 2. Gather Changes from Version Control

**Using GitHub:**
```bash
# List commits since last release
git log v0.8.3..HEAD --oneline

# List merged PRs
gh pr list --state merged --base main --json number,title
```

**Using GitHub Web:**
1. Navigate to repository
2. Go to **Pull requests** > **Closed**
3. Filter by milestone or date range

### 3. Categorize Changes

Group changes into standard categories:

| Category | Description |
|----------|-------------|
| **New Features** | New functionality |
| **Enhancements** | Improvements to existing features |
| **Bug Fixes** | Resolved issues |
| **Breaking Changes** | Changes requiring user action |
| **Deprecations** | Features being phased out |
| **Security Updates** | Security-related changes |

### 4. Write Clear Descriptions

**Good Example:**
```markdown
- Add support for GPT-4o model deployment in agents (#1234)
- Fix conversation history not persisting after browser refresh (#1235)
```

**Poor Example:**
```markdown
- Updated stuff
- Fixed bug
```

### 5. Document Breaking Changes

For each breaking change:

1. Describe what changed
2. Explain the impact
3. Provide migration steps
4. Note any automated migration

```markdown
## Breaking Changes

### Agent Configuration Format Change

The agent configuration schema has changed:

**Before (v0.8.x):**
```json
{
  "workflow": "langchain"
}
```

**After (v0.9.x):**
```json
{
  "workflow_object_id": "/instances/{id}/providers/FoundationaLLM.Agent/workflows/langchain"
}
```

**Migration:** Run the upgrade script `Migrate-AgentConfig.ps1` before deploying v0.9.x.
```

### 6. Include Upgrade Instructions

Provide clear upgrade steps:

```markdown
## Upgrade Instructions

### From v0.8.x to v0.9.x

1. Backup your deployment
2. Run pre-migration script
3. Update container images
4. Run post-migration script
5. Verify functionality

See [Upgrade Guide](./upgrade-guide.md) for detailed steps.
```

### 7. Review Process

1. **Self-review** - Check for accuracy and completeness
2. **Technical review** - Have developers verify changes
3. **Stakeholder review** - Get product owner approval
4. **Edit** - Incorporate feedback

### 8. Publish

**GitHub Release:**
1. Navigate to repository > **Releases**
2. Click **Draft a new release**
3. Select or create tag (e.g., `v0.9.0`)
4. Enter release title
5. Paste release notes
6. Attach any artifacts
7. Click **Publish release**

## Templates

### Minor Release Template

```markdown
# Release X.Y.0

**Release Date:** YYYY-MM-DD

## Highlights
- [Main feature or theme of this release]

## New Features
- 

## Enhancements
- 

## Bug Fixes
- 

## Upgrade Instructions
1. Pull latest container images
2. Update App Configuration (if needed)
3. Verify deployment

See [Upgrade Guide](link) for details.
```

### Patch Release Template

```markdown
# Release X.Y.Z

**Release Date:** YYYY-MM-DD

This is a patch release containing bug fixes.

## Bug Fixes
- 

## Upgrade Instructions
Update container images to tag `X.Y.Z`.
```

## Best Practices

| Practice | Description |
|----------|-------------|
| **Be Specific** | Include issue numbers and specific descriptions |
| **User Focus** | Write from the user's perspective |
| **Consistent Format** | Use the same structure for all releases |
| **Link Documentation** | Reference detailed docs where appropriate |
| **Test Instructions** | Verify upgrade steps work |

## Automation

Consider automating release note generation:

```yaml
# Example GitHub Action
name: Generate Release Notes
on:
  release:
    types: [created]
jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Generate notes
        run: |
          gh api repos/${{ github.repository }}/releases/generate-notes \
            -f tag_name=${{ github.ref_name }} \
            -f previous_tag_name=$(git describe --tags --abbrev=0 HEAD^)
```

## Related Topics

- [Updating Container Versions](updating-container-versions.md)
- [GitHub Releases](https://github.com/foundationallm/foundationallm/releases)
