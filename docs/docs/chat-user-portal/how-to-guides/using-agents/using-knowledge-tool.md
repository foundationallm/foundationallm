# Using the Knowledge Tool

Learn how the Knowledge Tool helps agents search and retrieve relevant information from documents and knowledge bases.

## What Is the Knowledge Tool?

The Knowledge Tool enables agents to search through documents and retrieve relevant information to answer your questions. Instead of relying solely on general knowledge, agents with this tool can:

- Search uploaded files for specific information
- Query organizational knowledge bases
- Retrieve relevant excerpts from documents
- Provide answers grounded in your actual data

## When the Knowledge Tool Is Active

The Knowledge Tool works automatically when:
- The agent has access to knowledge sources
- You've uploaded files to the conversation
- You ask questions about specific content

You don't need to explicitly activate it — the agent uses it when relevant to your question.

## How It Works

### The Search Process

1. **You ask a question** about a topic or document
2. **The agent searches** through available knowledge sources
3. **Relevant passages are retrieved** from documents
4. **The agent synthesizes** an answer using the retrieved information
5. **Sources may be cited** in the response

### Knowledge Sources

The Knowledge Tool can search:

| Source Type | Description |
|-------------|-------------|
| **Uploaded files** | Documents you've attached to the conversation |
| **Knowledge bases** | Pre-configured document collections |
| **Connected data sources** | Organizational repositories (SharePoint, data lakes, etc.) |

## What You Can Do

### Ask Questions About Documents

Upload a document and ask:
- "What does this report say about Q3 performance?"
- "Summarize the key findings from the uploaded study"
- "What recommendations does the document make?"
- "Find all mentions of [specific term] in this file"

### Search Across Multiple Files

When you upload several files:
- "Compare the conclusions across all uploaded reports"
- "Which document discusses pricing changes?"
- "What do these files say about the timeline?"

### Query Knowledge Bases

If the agent has access to knowledge bases:
- "What's our company policy on [topic]?"
- "Find documentation about [feature]"
- "What procedures should I follow for [task]?"

### Get Specific Information

Ask detailed questions:
- "What was the revenue in Q2 according to the financial report?"
- "Who is mentioned as the project lead in the document?"
- "What date was the policy last updated?"

## Example Prompts

### Document Summarization

> "I've uploaded our annual report. Please summarize the executive summary and highlight the three most important metrics."

### Specific Fact Finding

> "According to the uploaded employee handbook, how many vacation days do employees receive after their first year?"

### Cross-Document Analysis

> "Compare the project timelines mentioned in the two uploaded project plans and identify any conflicts."

### Knowledge Base Search

> "What does our IT documentation say about setting up VPN access?"

## Understanding Responses

### Grounded Answers

When the Knowledge Tool is used:
- Responses are based on actual document content
- The agent may quote or paraphrase source material
- Answers are limited to what's in the knowledge sources

### When Information Isn't Found

If the agent can't find relevant information:
- It will tell you the information wasn't found
- It may ask for clarification
- It might suggest alternative sources or approaches

### Content Artifacts

Some agents show "Content Artifacts" — clickable items that show:
- Source document titles
- Retrieved passages
- Metadata about the information source

Click on content artifacts to see details about where information came from.

## Tips for Best Results

### Ask Specific Questions

Instead of: "Tell me about this document"
Try: "What are the main conclusions in section 3 of the document?"

### Provide Context

- Mention which document you're asking about (if multiple are uploaded)
- Specify the type of information you need
- Include relevant keywords

### Use Clear File Names

When uploading files:
- Use descriptive names
- Reference files by name in your questions
- Organize related files together

### Start Broad, Then Narrow

1. First: "What topics does this document cover?"
2. Then: "Tell me more about [specific topic] from the document"
3. Finally: "What specific recommendations does it make about [detail]?"

## Limitations

### What the Knowledge Tool Cannot Do

- Search your local computer or systems outside the platform
- Access documents you haven't uploaded or that aren't in configured sources
- Read scanned images without OCR (text-as-images)
- Access real-time or frequently changing data

### Search Scope

- Searches only configured knowledge sources
- May not find information in poorly formatted documents
- Very large documents may have some sections not indexed

### Information Accuracy

- Answers depend on the quality of source documents
- The agent interprets and summarizes — verify critical information
- Outdated documents may provide outdated information

## Troubleshooting

### Agent Doesn't Find Information

**If the information should be there:**
- Rephrase your question using different terms
- Ask what topics the document covers
- Check if the document uploaded successfully
- Verify the document format is searchable (not a scanned image)

**If you're unsure:**
- Ask the agent what files it has access to
- Try uploading the document again
- Use simpler, more direct questions

### Incorrect Information Retrieved

- Ask for the source of the information
- Request direct quotes from the document
- Rephrase to be more specific about what you want
- Check if the agent is looking at the correct document

### Knowledge Base Not Available

- The agent may not have knowledge base access configured
- Try a different agent with knowledge base capabilities
- Contact your administrator about available knowledge sources

### Can't Find Specific Passages

- The passage may not be indexed correctly
- Try searching for key unique terms from the passage
- Ask about the general topic first
- Consider if the document format allows text extraction

## Security and Privacy

- Uploaded files are processed to extract searchable content
- Knowledge base access is controlled by administrators
- Search results are limited to what you're authorized to access
- Follow your organization's document handling policies

## Differences from Code Interpreter

| Feature | Knowledge Tool | Code Interpreter |
|---------|---------------|------------------|
| **Purpose** | Search and retrieve | Compute and analyze |
| **Output** | Text from documents | Calculated results, charts |
| **Best for** | Q&A about content | Data analysis |
| **File use** | Search text | Process data |

Use both tools together for powerful document analysis:
1. Upload data files
2. Use Knowledge Tool to understand the context
3. Use Code Interpreter to analyze the data

## Related Topics

- [Uploading Files to a Conversation](uploading-files.md) — Get documents into the conversation
- [Using the Code Interpreter Tool](using-code-interpreter.md) — Analyze data programmatically
- [Viewing Agent Prompts](viewing-agent-prompts.md) — See what context is used
- [Downloading Files from a Conversation](downloading-files.md) — Save generated outputs
