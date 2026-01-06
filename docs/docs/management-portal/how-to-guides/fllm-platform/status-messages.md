> **This article is still being authored.** Some sections contain placeholder content that requires additional information.

# Publishing Status Messages

Learn how to publish status, outage, and maintenance messages to inform users about system availability.

## Overview

Status messages allow IT administrators to communicate important information to users of the Chat User Portal. Use status messages to:

- Announce planned maintenance windows
- Notify users of ongoing outages or issues
- Communicate system updates or changes
- Provide important operational information

## Message Types

| Type | Icon | Purpose | Use When |
|------|------|---------|----------|
| **Information** | ℹ️ | General announcements | Sharing news, tips, or updates |
| **Warning** | ⚠️ | Potential issues | Planned maintenance, degraded performance |
| **Error/Outage** | ❌ | Active problems | System outages, critical issues |
| **Success** | ✅ | Resolution notices | Issues resolved, maintenance completed |

## Accessing Status Message Management

> **TODO**: Document the exact navigation path to status message management in the Management Portal.

1. Log into the Management Portal
2. Navigate to **FLLM Platform** > **Status Messages** (or equivalent)
3. The status message management page opens

## Creating a Status Message

### Step 1: Start New Message

1. Click **Create Status Message** or **New Message**
2. The message creation form opens

### Step 2: Configure Message Content

| Field | Description | Requirements |
|-------|-------------|--------------|
| **Title** | Brief headline for the message | Required. Keep concise. |
| **Message Body** | Full content of the status message | Required. Supports rich text. |
| **Message Type** | Severity/category of the message | Required. See types above. |
| **Priority** | Display priority if multiple messages | Optional. Higher = more prominent. |

### Step 3: Set Display Options

#### Visibility Settings

| Setting | Description |
|---------|-------------|
| **Start Date/Time** | When the message becomes visible |
| **End Date/Time** | When the message automatically hides |
| **Active** | Toggle to immediately show/hide the message |

#### Target Audience

> **TODO**: Document audience targeting options if available (e.g., all users, specific groups, specific agents).

### Step 4: Preview and Publish

1. Review the message content and settings
2. Use the preview option to see how it will appear
3. Click **Publish** or **Save** to activate the message

## Managing Existing Messages

### Viewing Active Messages

The status messages list shows:

| Column | Description |
|--------|-------------|
| **Title** | Message headline |
| **Type** | Message severity/category |
| **Status** | Active, Scheduled, or Expired |
| **Start/End** | Display window |
| **Actions** | Edit, Deactivate, Delete |

### Editing a Message

1. Find the message in the list
2. Click the **Edit** button
3. Modify content or settings
4. Save changes

Changes to active messages take effect immediately.

### Deactivating a Message

To temporarily hide a message without deleting it:

1. Find the message in the list
2. Click **Deactivate** or toggle the Active switch off
3. The message stops displaying in the User Portal

### Deleting a Message

1. Find the message in the list
2. Click the **Delete** button
3. Confirm the deletion

> **Warning:** Deleted messages cannot be recovered.

## Display Rules in User Portal

### Where Messages Appear

Status messages are displayed in the Chat User Portal:

> **TODO**: Document specific display locations (e.g., banner at top, notification area, login page).

- **Banner Display**: Messages may appear as banners at the top of the portal
- **Notification Area**: Users may see notifications in the sidebar
- **Login Page**: Critical messages may appear before login

### Message Behavior

| Behavior | Description |
|----------|-------------|
| **Auto-dismiss** | Some message types may auto-hide after being read |
| **Persistent** | Critical messages may stay visible until resolved |
| **Dismissible** | Users may be able to dismiss informational messages |

### Multiple Messages

When multiple messages are active:

- Higher priority messages display more prominently
- Messages may stack or rotate
- Critical messages typically take precedence

## Scheduling Messages

### Planning Ahead

For planned maintenance:

1. Create the message in advance
2. Set the **Start Date/Time** to when you want it visible
3. Set the **End Date/Time** to when maintenance ends (plus buffer)
4. The message will automatically appear and disappear

### Maintenance Window Example

For a planned maintenance window on Saturday 2 AM - 6 AM:

| Setting | Value |
|---------|-------|
| Title | Scheduled Maintenance |
| Message | The system will be unavailable for scheduled maintenance. |
| Type | Warning |
| Start | Saturday 1:45 AM (15 min early notice) |
| End | Saturday 6:30 AM (30 min buffer) |

## Best Practices

### Message Content

- **Be Clear**: Use plain language everyone can understand
- **Be Specific**: Include times, durations, and affected services
- **Be Actionable**: Tell users what they should do (wait, contact support, etc.)
- **Be Timely**: Update messages as situations evolve

### Message Timing

| Scenario | Advance Notice |
|----------|----------------|
| Major planned maintenance | 1-2 weeks |
| Minor maintenance | 2-3 days |
| Emergency maintenance | As soon as known |
| Outage notification | Immediately |

### Message Management

- Review and clean up old messages regularly
- Use scheduling for recurring maintenance
- Have templates ready for common scenarios
- Coordinate with support teams on messaging

## Templates

### Planned Maintenance

```
Title: Scheduled Maintenance - [Date]

The FoundationaLLM system will be unavailable for scheduled maintenance 
on [Date] from [Start Time] to [End Time] [Timezone].

During this time, the Chat User Portal will not be accessible. 
Please plan your work accordingly.

We apologize for any inconvenience.
```

### Active Outage

```
Title: Service Disruption - [Brief Description]

We are currently experiencing issues with [affected service/feature].
Our team is actively working to resolve this issue.

Status: [Investigating/Identified/Fixing]
Estimated Resolution: [Time if known, or "TBD"]

We will provide updates as more information becomes available.
```

### Resolution Notice

```
Title: Resolved - [Original Issue]

The [issue description] reported on [date/time] has been resolved.
Normal service has been restored.

Thank you for your patience.
```

## Troubleshooting

### Message Not Appearing

- Verify the message is set to **Active**
- Check the Start Date/Time hasn't passed incorrectly
- Confirm the message hasn't been filtered by audience settings
- Clear browser cache and refresh the User Portal

### Message Won't Save

- Check all required fields are filled
- Verify date/time formats are correct
- Ensure you have appropriate permissions

### Users Not Seeing Updates

- Changes may take a few minutes to propagate
- Users may need to refresh their browser
- Check if caching is affecting message display

## Related Topics

- [Branding Configuration](branding.md) — Customize portal appearance
- [Configuration Settings](configuration.md) — Platform configuration options
- [Viewing Status Messages](../../../chat-user-portal/how-to-guides/using-agents/viewing-status-messages.md) — End-user guide
