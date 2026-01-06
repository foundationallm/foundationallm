# System Status Messages

System status messages allow IT teams to communicate important information, maintenance schedules, and outage notifications to User Portal users.

## Overview

Status messages appear in the User Portal to notify users about:

- Scheduled maintenance windows
- Active service disruptions
- New feature announcements
- Important system changes

## Message Types

### Severity Levels

| Level | Use Case | Visual Style |
|-------|----------|--------------|
| **Information** | General announcements, tips | Blue banner |
| **Warning** | Upcoming maintenance, degraded service | Yellow/orange banner |
| **Critical** | Active outages, urgent issues | Red banner |

### Display Behavior

| Severity | Dismissible | Persistence |
|----------|-------------|-------------|
| Information | Yes | User can dismiss |
| Warning | Configurable | Based on settings |
| Critical | No | Remains until removed |

<!-- [TODO: Confirm dismissal behavior per severity level] -->

## Creating Status Messages

### Access Requirements

To publish status messages, you need:

- Access to the Management Portal
- Administrator permissions for status messages

### Step-by-Step Creation

1. **Navigate to Status Messages**
   - Open Management Portal
   - Go to **System** > **Status Messages**

2. **Create New Message**
   - Click **New Message**
   - Fill in the required fields

3. **Configure Content**

| Field | Description | Required |
|-------|-------------|----------|
| **Title** | Brief headline | Yes |
| **Message** | Full message text | Yes |
| **Severity** | Info/Warning/Critical | Yes |

4. **Set Schedule**

| Option | Description |
|--------|-------------|
| **Show Immediately** | Display right away |
| **Start Date/Time** | Schedule for future |
| **End Date/Time** | Auto-expire the message |

5. **Preview and Publish**
   - Click **Preview** to see how it appears
   - Click **Publish** to make it live

<!-- [TODO: Add screenshots of creation process] -->

## Display Rules

### Placement

Status messages appear at the top of the User Portal, above the main content area.

### Multiple Messages

When multiple messages are active:

1. Messages display in severity order (critical first)
2. Each message appears in its own banner
3. Maximum of 3 messages shown simultaneously

<!-- [TODO: Confirm maximum message display limit] -->

### User Interaction

| Interaction | Behavior |
|-------------|----------|
| **Dismiss** | Hides message for session (if dismissible) |
| **Refresh page** | Dismissed messages may reappear |
| **New session** | All active messages displayed |

## Managing Messages

### Viewing Active Messages

1. Navigate to **System** > **Status Messages**
2. View the **Active Messages** list
3. See status, audience, and schedule

### Editing Messages

1. Find the message in the list
2. Click **Edit**
3. Modify content or schedule
4. Save changes

Changes take effect immediately.

### Removing Messages

1. Find the message
2. Click **Remove** or **End Now**
3. Confirm the action

Message disappears from User Portal within minutes.

### Viewing Message History

1. Navigate to **Status Messages**
2. Click **History** tab
3. View past messages with:
   - Publication dates
   - Duration displayed
   - Who published

## Message Content Best Practices

### Writing Effective Messages

| Do | Don't |
|----|-------|
| Be specific about timing | Use vague language |
| Include impact description | Assume users understand context |
| Provide action items if needed | Write excessive detail |
| Use plain language | Use technical jargon |

### Example Messages

#### Scheduled Maintenance

```
Title: Scheduled Maintenance - Saturday Night

The FoundationaLLM platform will be unavailable for scheduled 
maintenance on Saturday, January 15th from 11:00 PM to 3:00 AM EST. 

During this time, the chat service will be unavailable. We apologize 
for any inconvenience.
```

#### Active Outage

```
Title: Service Disruption

We are currently experiencing issues with the chat service. 
Our team is actively working to resolve the problem. 

Status updates will be posted here.
Last updated: 2:45 PM EST
```

#### Feature Announcement

```
Title: New Feature: Image Generation

You can now ask your agent to generate images! Try saying 
"Create an image of..." to use this new capability.
```

## Scheduling Messages

### Advance Scheduling

For planned maintenance, schedule messages in advance:

1. Set **Start Date/Time** to when message should appear
2. Set **End Date/Time** to when message should expire
3. Message automatically appears and disappears

### Time Zone Handling

<!-- [TODO: Document how time zones are handled] -->

| Consideration | Behavior |
|---------------|----------|
| Display time | TBD |
| User local time | TBD |
| UTC conversion | TBD |

## API Reference

### List Active Messages

```http
GET {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/statusMessages
```

### Create Message

```http
POST {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/statusMessages
Content-Type: application/json

{
  "title": "Scheduled Maintenance",
  "message": "The system will be unavailable...",
  "severity": "warning",
  "startDateTime": "2025-01-15T23:00:00Z",
  "endDateTime": "2025-01-16T03:00:00Z"
}
```

### Remove Message

```http
DELETE {{baseUrl}}/instances/{{instanceId}}/providers/FoundationaLLM.Configuration/statusMessages/{messageId}
```

<!-- [TODO: Verify API endpoints and schema] -->

## Permissions

### Required Roles

| Role | View Messages | Publish Messages | Edit Messages |
|------|---------------|------------------|---------------|
| Reader | ✅ | ❌ | ❌ |
| Contributor | ✅ | ✅ | ✅ |
| Administrator | ✅ | ✅ | ✅ |

<!-- [TODO: Confirm specific RBAC requirements] -->

## Troubleshooting

### Message Not Appearing

1. Verify message is published (not draft)
2. Check start date/time has passed
3. Verify message hasn't expired
4. Clear browser cache in User Portal

### Message Won't Dismiss

1. Critical messages cannot be dismissed
2. Check severity setting
3. Verify dismissal behavior configuration

### Scheduling Issues

1. Verify time zone settings
2. Ensure end time is after start time
3. Check for overlapping messages

## Related Topics

- [Status Message Publishing Walkthrough](../user-portal/walkthroughs/status-message-walkthrough.md)
- [Branding Configuration](../branding/index.md)
- [User Portal Guide](../user-portal/index.md)
