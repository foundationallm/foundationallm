# Uploading Files to a Conversation

Share documents, images, and other files with agents for analysis and reference.

## When to Upload Files

Upload files when you want an agent to:

- Analyze data in spreadsheets or CSV files
- Answer questions about document content
- Process or transform data
- Generate visualizations from your data
- Extract information from images
- Reference specific files in responses

> **Note:** Not all agents support file uploads. The upload feature only appears if the agent is configured to accept files.

## Prerequisites

Before uploading files:

- **Agent must support uploads**: Check if the paperclip icon appears in the chat input
- **Supported file format**: Your file must be in a format the agent can process
- **File size limits**: Files must be within size limits set by your administrator

## Supported File Types

Common supported formats include:

| Category | File Types |
|----------|------------|
| **Documents** | PDF, DOCX, DOC, TXT, MD, RTF |
| **Spreadsheets** | XLSX, XLS, CSV, TSV |
| **Images** | PNG, JPG, JPEG, GIF, BMP, TIFF |
| **Code Files** | PY, JS, TS, JSON, XML, HTML, CSS |
| **Archives** | ZIP, TAR |
| **Presentations** | PPTX, PPT |

> **Note:** Supported types vary by agent configuration. Some agents may support additional formats or have restrictions.

## How to Upload Files

### Step 1: Open the Upload Panel

1. Look for the **paperclip icon** (📎) to the left of the message input box
2. Click the paperclip to open the file upload panel
3. A badge on the icon shows how many files are currently attached

### Step 2: Select Files

You have two options:

#### From Your Computer

1. Click **Select file from Computer**
2. Browse to find your file
3. Select one or more files
4. Click **Open**

#### From OneDrive (If Available)

1. If you see **Connect to OneDrive**, click it first
2. Authorize access to your OneDrive
3. Click **Select file from OneDrive**
4. Browse and select files from your OneDrive
5. The OneDrive picker shows your available files

### Step 3: Review Selected Files

Before uploading, selected files appear in the panel with:

| Information | Description |
|-------------|-------------|
| **File name** | The name of your file |
| **File size** | Size displayed for local files |
| **Source badge** | "Local Computer" or "OneDrive Work/School" |
| **Status badge** | "Pending" (not yet uploaded) |
| **Remove button** | X button to remove the file |

### Step 4: Upload the Files

1. Review your selected files
2. Click the **Upload** button
3. A progress bar shows upload status
4. Wait for "Uploaded" badges to appear
5. Successfully uploaded files show a green "Uploaded" badge

### Step 5: Close the Panel

1. Click **Close** to close the upload panel
2. The paperclip badge shows your total attached files
3. Send a message to start working with the files

## Managing Uploaded Files

### Viewing Attached Files

- Click the paperclip icon to see all files for the current conversation
- Files are listed with their status and source

### Removing Files

To remove a file before sending a message:
1. Open the upload panel (click paperclip)
2. Find the file you want to remove
3. Click the **X** button next to the file
4. Confirm removal if prompted

### File Persistence

- Uploaded files are attached to the current conversation
- They remain available throughout the conversation
- Starting a new conversation clears the attachment list

## OneDrive Integration

If your organization has enabled OneDrive integration:

### Connecting OneDrive

1. Click **Connect to OneDrive** in the upload panel
2. Sign in with your work or school account
3. Grant necessary permissions
4. Once connected, you'll see **Select file from OneDrive**

### Selecting from OneDrive

1. Click **Select file from OneDrive**
2. A file picker opens showing your OneDrive contents
3. Navigate to find your file
4. Select one or more files
5. Confirm your selection

### Disconnecting OneDrive

1. Open the upload panel
2. Click **Disconnect OneDrive**
3. Your OneDrive access is revoked
4. You can reconnect anytime

## After Uploading

Once files are uploaded:

### Referencing Files

- Simply mention the file in your message
- Ask questions about the content
- Request analysis or processing

### Example Prompts

- "Analyze the sales data in the uploaded spreadsheet"
- "Summarize the key points from the uploaded document"
- "What trends do you see in this CSV file?"
- "Extract all email addresses from the uploaded text file"
- "Create a chart from the data I uploaded"

### How Agents Use Files

Agents process uploaded files by:
1. Extracting text and data content
2. Making the content available for reference
3. Using the information to answer your questions
4. Potentially generating new files based on your data

## File Size Limits

Limits vary by configuration:

| Consideration | Typical Behavior |
|---------------|------------------|
| **Per-file limit** | Usually 10-50 MB per file |
| **Total per message** | Often limited to 10 files |
| **Large files** | May take longer to process |

Check with your administrator for specific limits in your environment.

## Troubleshooting

### Paperclip Icon Not Showing

- The current agent doesn't support file uploads
- Try selecting a different agent
- Contact your administrator if you need upload capability

### Upload Fails

- Check that the file is within size limits
- Verify the file format is supported
- Try a different file to isolate the issue
- Check your internet connection

### File Format Not Accepted

- Convert the file to a supported format
- PDF is widely supported for documents
- CSV works well for data files
- PNG or JPEG for images

### OneDrive Connection Issues

- Ensure you have a work/school OneDrive account
- Check that your organization allows OneDrive integration
- Try disconnecting and reconnecting
- Clear browser cache and try again

### File Not Being Referenced

If the agent doesn't seem to see your file:
- Make sure the upload completed (shows "Uploaded" badge)
- Reference the file specifically in your message
- Upload may need to complete before sending your question
- Try asking "What files do I have uploaded?"

### Progress Bar Stuck

- Check your internet connection
- Wait a moment — large files take time
- If stuck for a long time, refresh the page and try again
- Very large files may timeout

### Files Missing After Refresh

- Uploaded files are tied to the current conversation
- Switching agents clears attachments
- Re-upload files if needed after refreshing

## Tips for Better Results

### Prepare Your Files

- Remove unnecessary data from spreadsheets
- Use clear column headers
- Ensure text is readable (not scanned images of text)
- Smaller, focused files process faster

### Be Specific in Your Questions

- Reference the file by name if you have multiple
- Specify what analysis or output you want
- Ask about specific columns, sections, or data

### Multiple Files

- Upload related files together
- Mention how files relate to each other
- Consider combining data into one file if appropriate

### Sensitive Data

- Be mindful of what data you upload
- Uploaded files are processed by AI systems
- Follow your organization's data handling policies

## Related Topics

- [Downloading Files from a Conversation](downloading-files.md) — Save files agents generate
- [Using the Code Interpreter Tool](using-code-interpreter.md) — Analyze uploaded data
- [Using the Knowledge Tool](using-knowledge-tool.md) — Search uploaded documents
