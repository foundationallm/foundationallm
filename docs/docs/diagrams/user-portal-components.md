# UserPortal Component Architecture

This document describes the Vue.js component structure of the FoundationaLLM UserPortal and their screen coverage.

## Screen Layout Diagram

```mermaid
block-beta
    columns 1
    
    block:browser["Browser Window"]
        columns 1
        
        block:navbar["NavBar.vue"]
            columns 3
            logo["Logo + Sidebar Toggle"]
            sessionName["Session Display Name"]
            agentDropdown["Agent Dropdown + Print<br/>(AgentIcon.vue)"]
        end
        
        block:mainArea
            columns 2
            
            block:sidebar["ChatSidebar.vue"]:1
                columns 1
                newChat["+ New Chat Button"]
                session1["Session 1 (current)"]
                session2["Session 2"]
                session3["Session 3"]
                sessionN["..."]
                settings["⚙️ Settings Button"]
            end
            
            block:chatArea["ChatThread.vue"]:1
                columns 1
                block:messages["Message List Area"]
                    columns 1
                    msg1["ChatMessage.vue (User)<br/>UserAvatar + ChatMessageTextBlock"]
                    msg2["ChatMessage.vue (Agent)<br/>AgentIcon + ChatMessageContentBlock"]
                end
                block:input["ChatInput.vue"]
                    columns 1
                    editor["CustomQuillEditor.vue"]
                    attachments["AttachmentList.vue + Send Button"]
                end
            end
        end
    end
```

## Modal Overlays

These components render as modal dialogs on top of the main UI:

```mermaid
graph TB
    subgraph Modals["Modal Overlay Components"]
        NavBarSettings["<b>NavBarSettings.vue</b><br/>Settings modal<br/>• Auto-hide toasts toggle<br/>• Text size slider<br/>• High contrast mode<br/>• Agent enable/disable list"]
        AnalysisModal["<b>AnalysisModal.vue</b><br/>View prompt/analysis details"]
        ConfirmationDialog["<b>ConfirmationDialog.vue</b><br/>Delete session confirmation"]
        SessionExpiration["<b>SessionExpirationDialog.vue</b><br/>Session timeout warning"]
    end
```

## Component Hierarchy

```mermaid
graph TD
    app["app.vue"]
    app --> index["pages/index/index.vue"]
    
    index --> NavBar["NavBar.vue"]
    index --> ChatSidebar["ChatSidebar.vue"]
    index --> ChatThread["ChatThread.vue"]
    
    NavBar --> AgentIcon1["AgentIcon.vue"]
    
    ChatSidebar --> NavBarSettings["NavBarSettings.vue<br/>(modal)"]
    ChatSidebar --> TimeAgo["TimeAgo.vue"]
    
    ChatThread --> ChatMessage["ChatMessage.vue (×N)"]
    ChatThread --> ChatInput["ChatInput.vue"]
    ChatThread --> ConfirmationDialog["ConfirmationDialog.vue"]
    
    ChatMessage --> UserAvatar["UserAvatar.vue"]
    ChatMessage --> ChatMessageTextBlock["ChatMessageTextBlock.vue"]
    ChatMessage --> ChatMessageContentBlock["ChatMessageContentBlock.vue"]
    ChatMessage --> AnalysisModal["AnalysisModal.vue"]
    
    ChatMessageContentBlock --> ChatMessageContentArtifactBlock["ChatMessageContentArtifactBlock.vue"]
    ChatMessageContentArtifactBlock --> CodeBlockHeader["CodeBlockHeader.vue"]
    
    ChatInput --> CustomQuillEditor["CustomQuillEditor.vue"]
    ChatInput --> AttachmentList["AttachmentList.vue"]
```

## Component Responsibilities

| Component | Screen Area | Purpose |
|-----------|-------------|---------|
| **NavBar.vue** | Top bar (fixed) | Logo, sidebar toggle, session name, agent dropdown, print |
| **ChatSidebar.vue** | Left panel (collapsible) | Session list, new chat, settings, session management |
| **ChatThread.vue** | Main content area | Message display, welcome message, loading states |
| **ChatMessage.vue** | Message bubbles | Individual message rendering (user or agent) |
| **ChatInput.vue** | Bottom of main area | Text input, attachments, send button |
| **NavBarSettings.vue** | Modal overlay | User preferences, agent toggles |
| **ConfirmationDialog.vue** | Modal overlay | Delete confirmations |

## File Locations

All components are located in `src/ui/UserPortal/components/`:

- `AgentIcon.vue`
- `AnalysisModal.vue`
- `AttachmentList.vue`
- `ChatInput.vue`
- `ChatMessage.vue`
- `ChatMessageContentArtifactBlock.vue`
- `ChatMessageContentBlock.vue`
- `ChatMessageTextBlock.vue`
- `ChatSidebar.vue`
- `ChatThread.vue`
- `CodeBlockHeader.vue`
- `ConfirmationDialog.vue`
- `CustomQuillEditor.vue`
- `NavBar.vue`
- `NavBarSettings.vue`
- `SessionExpirationDialog.vue`
- `TimeAgo.vue`
- `UserAvatar.vue`

## State Management

The UserPortal uses Pinia stores located in `src/ui/UserPortal/stores/`:

| Store | Purpose |
|-------|---------|
| **appStore.ts** | Sessions, messages, agents, attachments, user preferences |
| **appConfigStore.ts** | Application configuration (logo, featured agents, kiosk mode) |
| **authStore.ts** | Authentication state and user account |
| **confirmationStore.ts** | Confirmation dialog state |
