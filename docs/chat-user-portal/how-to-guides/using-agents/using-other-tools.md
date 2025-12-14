# Using Other Tools

Learn about additional tools and capabilities that may be available to agents beyond Code Interpreter and Knowledge tools.

## Understanding Agent Tools

Agents in FoundationaLLM can be equipped with various tools that extend their capabilities. While Code Interpreter and Knowledge tools are common, agents may have access to:

- Image generation tools
- Custom organizational tools
- External API integrations
- Specialized processing capabilities

## How Tools Work

### Automatic Tool Selection

You don't need to manually select which tool to use:

1. **Describe what you need** in natural language
2. **The agent determines** which tool is appropriate
3. **The tool executes** and returns results
4. **Results appear** in the conversation

### Tool Transparency

Some agents show information about tool usage:
- Tool names may appear in responses
- Processing indicators show when tools are working
- Results clearly indicate they came from a tool

## Image Generation

Some agents can create images using tools like DALL-E.

### When to Use

- Creating illustrations or graphics
- Visualizing concepts
- Generating placeholder images
- Creating custom visuals

### How to Request Images

Be descriptive in your request:
- "Create an image of a modern office building at sunset"
- "Generate a diagram showing the project workflow"
- "Design a logo concept with blue and green colors"

### Image Results

Generated images:
- Appear directly in the conversation
- Can be clicked for full-size preview
- Can be downloaded by right-clicking

### Tips for Better Images

- Be specific about style, colors, and composition
- Mention the intended use (icon, background, illustration)
- Request revisions by describing what to change
- Note: photorealistic images may have limitations

## Custom Organizational Tools

Your organization may have deployed custom tools for specific purposes.

### Examples of Custom Tools

| Tool Type | Purpose |
|-----------|---------|
| **Database queries** | Access internal data systems |
| **API integrations** | Connect to external services |
| **Document processing** | Handle specific document formats |
| **Workflow automation** | Trigger organizational processes |
| **Specialized calculations** | Industry-specific computations |

### Discovering Custom Tools

To learn what custom tools an agent has:
- Ask the agent: "What tools do you have access to?"
- Check the agent description in Settings > Agents
- Contact your administrator for documentation

### Using Custom Tools

Custom tools work like any other tool:
1. Describe what you need
2. The agent uses the appropriate tool
3. Results appear in the response

## External API Integrations

Some agents can connect to external services:

- Weather services
- Stock market data
- News aggregators
- Social media platforms
- Business applications

### Important Considerations

- API calls may have rate limits
- Real-time data depends on API availability
- Results are as current as the API provides
- Some integrations may require authentication

## Specialized Processing

Agents may have specialized capabilities:

### Language Processing
- Translation between languages
- Sentiment analysis
- Text summarization
- Named entity recognition

### Document Processing
- PDF extraction
- Form parsing
- Table recognition
- Metadata extraction

### Audio/Visual Processing
- Image analysis and description
- Audio transcription
- Visual element detection

## Tips for Using Tools Effectively

### Be Specific About What You Need

Good: "Generate a pie chart showing Q3 sales by region"
Not as good: "Make a chart"

### Ask About Capabilities

If you're unsure what an agent can do:
- "What tools or capabilities do you have?"
- "Can you access [specific system/data]?"
- "What types of files can you process?"

### Provide Context

- Explain why you need something
- Share relevant background
- Specify the output format you want

### Iterate and Refine

- Start with a basic request
- Ask for modifications based on results
- Build on successful outputs

## Understanding Tool Limitations

### What Tools Cannot Do

- Access systems they're not connected to
- Bypass security controls
- Perform actions outside their scope
- Return real-time data without real-time connections

### Response Time

Tool execution may take time depending on:
- The complexity of the operation
- External service response times
- Data processing requirements

A loading indicator shows when tools are working.

## Troubleshooting

### Tool Doesn't Seem to Work

- The agent may not have that tool available
- Try rephrasing your request
- Ask what capabilities the agent has
- Try a different agent

### Unexpected Results

- Be more specific in your request
- Ask the agent to explain what it did
- Try breaking the task into smaller steps
- Verify your input data

### Tool Response is Slow

- Complex operations take longer
- External APIs may have delays
- Large data processing requires time
- Wait for the response to complete

### Tool Returns an Error

- Check if your request is within the tool's capabilities
- Verify any input data is correct
- Try a simpler version of the request
- Ask the agent what went wrong

## Security and Access

Tools operate within security boundaries:
- Access is controlled by administrators
- User permissions affect what tools can do
- Sensitive operations may require additional authentication
- Actions are logged for security auditing

## Learning More

### Documentation

- Check organizational documentation for custom tools
- Review agent descriptions for capability details
- Ask your administrator about available integrations

### Experimentation

- Try different types of requests
- Ask agents about their capabilities
- Test tools with sample data first

## Related Topics

- [Using the Code Interpreter Tool](using-code-interpreter.md) — Data analysis and visualization
- [Using the Knowledge Tool](using-knowledge-tool.md) — Document search and retrieval
- [Selecting an Agent](selecting-agent.md) — Choose agents with the right tools
- [Viewing Agent Prompts](viewing-agent-prompts.md) — Understand how agents work
