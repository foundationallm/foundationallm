# Status Message Publishing Walkthrough

This step-by-step walkthrough guides IT administrators through publishing system status messages to notify users about maintenance, outages, or important announcements.

> [!NOTE]
> Status message publishing is performed in the **Management Portal**, not the User Portal.

## Prerequisites

- Access to the FoundationaLLM Management Portal
- Administrator permissions to publish status messages

## Steps

### Step 1: Access the Management Portal

1. Navigate to your organization's Management Portal URL
2. Log in with your administrator credentials

<!-- [TODO: Add screenshot of Management Portal login] -->

### Step 2: Navigate to Status Messages

1. In the side navigation, locate **System** or **Settings**
2. Click on **Status Messages**

<!-- [TODO: Add screenshot of navigation to Status Messages] -->

### Step 3: View Existing Messages

The Status Messages page displays:

- Active messages currently showing to users
- Scheduled messages pending display
- Message history

<!-- [TODO: Add screenshot of Status Messages list] -->

### Step 4: Create a New Message

1. Click the **New Message** button
2. The message creation form opens

<!-- [TODO: Add screenshot of New Message button] -->

### Step 5: Configure Message Content

Fill in the message details:

| Field | Description | Example |
|-------|-------------|---------|
| **Title** | Brief headline | "Scheduled Maintenance" |
| **Message** | Full message text (HTML supported) | "The system will be unavailable..." |
| **Severity** | Message importance level | Info / Warning / Critical |

<!-- [TODO: Add screenshot of message content form] -->

#### Severity Levels

| Level | Use Case | Display Style |
|-------|----------|--------------|
| **Info** | General announcements | Blue banner |
| **Warning** | Upcoming maintenance | Yellow banner |
| **Critical** | Active outages | Red banner |

### Step 6: Set Display Schedule

Configure when the message appears:

| Option | Description |
|--------|-------------|
| **Start Date/Time** | When to begin showing the message |
| **End Date/Time** | When to stop showing the message |
| **Show Immediately** | Display the message right away |

<!-- [TODO: Add screenshot of schedule configuration] -->

### Step 7: Preview the Message

Before publishing:

1. Click the **Preview** button
2. See how the message will appear to users
3. Verify formatting and content

<!-- [TODO: Add screenshot of message preview] -->

### Step 8: Publish the Message

1. Review all settings
2. Click **Publish** or **Schedule**
3. Confirm the action in the dialog

<!-- [TODO: Add screenshot of publish confirmation] -->

### Step 9: Verify in User Portal

1. Open the User Portal in a new browser window
2. Verify the message appears correctly
3. Check placement and styling

<!-- [TODO: Add screenshot of message in User Portal] -->

### Step 10: Edit or Remove (If Needed)

To modify an active message:

1. Return to Status Messages in Management Portal
2. Find the message in the active list
3. Click **Edit** to modify or **Remove** to delete
4. Confirm any changes

<!-- [TODO: Add screenshot of edit/remove actions] -->

## Message Display Rules

Status messages in the User Portal follow these display rules:

| Rule | Behavior |
|------|----------|
| **Placement** | Messages appear at the top of the User Portal |
| **Multiple Messages** | Displayed in severity order (critical first) |
| **Dismissal** | Users can dismiss Info messages; Warning/Critical persist |
| **Duration** | Messages show until end time or manual removal |
| **Page Load** | Messages re-appear on new sessions |

<!-- [TODO: Confirm and document actual display rules] -->

## Result

After completing this walkthrough, you have:

- ✅ Accessed status message management
- ✅ Created a new status message
- ✅ Configured severity and schedule
- ✅ Published and verified the message

## Common Scenarios

### Scheduled Maintenance

```
Title: Scheduled Maintenance
Severity: Warning
Message: The FoundationaLLM platform will be undergoing scheduled 
maintenance on [Date] from [Start Time] to [End Time] UTC. 
The system may be temporarily unavailable during this period.
Schedule: Start 24 hours before maintenance
```

### Active Outage

```
Title: Service Disruption
Severity: Critical
Message: We are currently experiencing a service disruption 
affecting chat functionality. Our team is actively working 
to resolve the issue. We apologize for the inconvenience.
Schedule: Show Immediately
```

### Feature Announcement

```
Title: New Feature Available
Severity: Info
Message: We've added a new image generation capability! 
Try asking your agent to create images.
Schedule: Display for 1 week
```

## Best Practices

1. **Be concise**: Users skim messages; keep them brief
2. **Be specific**: Include dates, times, and expected impact
3. **Set appropriate severity**: Reserve Critical for actual outages
4. **Schedule in advance**: For planned maintenance, notify users early
5. **Remove promptly**: Don't leave outdated messages active

## Troubleshooting

### Message Not Appearing

- Verify the start date/time has passed
- Check that you clicked Publish (not just Save)
- Clear browser cache in User Portal

### Message Won't Dismiss

- Critical messages may be configured as non-dismissible
- Check the message settings

### Scheduling Issues

- Verify timezone settings are correct
- Ensure end time is after start time

## Next Steps

- Return to [Management UI Overview](../../management-ui/management-ui.md)
- Review [System Status Messages Documentation](../../management-ui/system-status-messages.md)

## Video Tutorial

<!-- [TODO: Add link to video walkthrough when available] -->
