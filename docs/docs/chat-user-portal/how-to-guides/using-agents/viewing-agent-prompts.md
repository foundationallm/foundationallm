# Viewing Agent Prompts

Learn how to view the prompts used by agents when generating responses.

## What Are Agent Prompts?

When you send a message to an agent, a "prompt" is created that includes:

- **System instructions**: The agent's core configuration and persona
- **Context**: Relevant information retrieved from knowledge sources
- **Conversation history**: Previous messages for continuity
- **Your message**: The question or request you submitted

Viewing prompts helps you understand how the agent generates its responses.

## When Prompt Viewing Is Available

The "View Prompt" feature appears when:
- Your administrator has enabled prompt viewing
- The agent is configured to allow prompt visibility
- The response has finished generating

Not all agents expose prompts — this is controlled by configuration.

## How to View a Prompt

### Step 1: Find the View Prompt Button

1. Look at the bottom of an **agent response** (not your own messages)
2. Find the message footer area where buttons appear
3. Look for the **View Prompt** button (shows a book icon 📖)

### Step 2: Open the Prompt Dialog

1. Click **View Prompt**
2. A dialog opens titled "Completion Prompt"
3. The full prompt text is displayed

### Step 3: Review the Content

The prompt dialog shows the text that was sent to the AI model to generate the response you received.

### Step 4: Close the Dialog

Click **Close** to dismiss the prompt dialog and return to the conversation.

## Understanding Prompt Contents

### System Instructions

The beginning of the prompt typically contains:
- The agent's role and persona
- Guidelines for how to respond
- Any restrictions or rules
- Formatting preferences

### Retrieved Context

If the Knowledge Tool was used:
- Relevant passages from documents appear
- Information from knowledge bases is included
- This provides the "grounding" for the response

### Conversation History

Recent messages may be included:
- Your previous questions
- The agent's previous answers
- This maintains continuity

### Your Current Message

Your most recent question or request appears, forming the actual query the agent needs to address.

## Why View Prompts?

### Debugging Responses

Understanding the prompt helps when:
- Responses aren't what you expected
- The agent seems to miss information
- You want to know what context was used

### Learning How Agents Work

Prompts reveal:
- How system instructions shape behavior
- What knowledge was retrieved
- How conversation context is used

### Verifying Context

Check that:
- The right documents were searched
- Relevant information was retrieved
- The agent had proper context

### Improving Your Questions

See how your input fits into the larger prompt to:
- Phrase questions more effectively
- Provide better context upfront
- Avoid redundant information

## Prompt Components Explained

| Component | Description | Purpose |
|-----------|-------------|---------|
| **System prompt** | Agent's base instructions | Defines agent behavior and capabilities |
| **Knowledge context** | Retrieved document excerpts | Grounds responses in specific information |
| **Conversation history** | Previous messages | Maintains coherent dialogue |
| **User message** | Your current input | The question being answered |

## When View Prompt Is Not Available

You won't see the View Prompt option if:

- **Feature disabled**: Your administrator hasn't enabled it
- **Agent configuration**: The specific agent doesn't allow prompt viewing
- **Message type**: Only agent responses have viewable prompts (not your messages)
- **Still loading**: Wait for the response to complete

## Tips for Using View Prompt

### Compare Prompts Across Responses

- View prompts for multiple responses
- Notice how context changes based on your questions
- See how conversation history accumulates

### Check Knowledge Retrieval

- See what documents were searched
- Verify relevant passages were found
- Understand why certain information was used

### Understand Response Limitations

If a response was limited or incomplete:
- Check if the relevant information was in the prompt
- See if context was missing
- Understand what the agent had to work with

### Verify Agent Behavior

- Confirm the agent is following its instructions
- Check that appropriate guidelines are being applied
- Understand the agent's configured capabilities

## Troubleshooting

### View Prompt Button Not Showing

- The feature may be disabled for your environment
- The specific agent may not support prompt viewing
- Wait for the response to fully complete
- Contact your administrator to request access

### Prompt Text Is Very Long

- Prompts can be lengthy, especially with lots of context
- Scroll through the dialog to see all content
- The dialog is scrollable

### Can't Understand the Prompt

Prompts may contain technical elements:
- System instructions are often detailed
- Formatting may include special tokens
- Focus on the parts relevant to your question

### Prompt Seems Incomplete

- Some prompt components may be summarized
- Very long contexts may be truncated
- Technical details may be omitted from the display

## Privacy Considerations

Prompts may contain:
- System configurations (visible by design)
- Retrieved document content
- Previous conversation messages
- Your user inputs

Follow organizational guidelines for handling prompt information.

## Using Prompts for Better Results

### Understand What Helps

By viewing prompts, you can learn:
- What type of context improves responses
- How much conversation history matters
- What format of questions works best

### Adjust Your Approach

Based on prompt insights:
- Provide more specific questions
- Upload relevant documents
- Reference key terms the agent needs
- Maintain conversation focus

### Work Within Limitations

If prompts reveal constraints:
- Work around token limits
- Provide concise inputs
- Focus on essential information

## Related Topics

- [Rating Responses](rating-responses.md) — Provide feedback on agent outputs
- [Using the Knowledge Tool](using-knowledge-tool.md) — Understand context retrieval
- [Monitoring Token Consumption](monitoring-tokens.md) — Track prompt sizes
- [Managing Conversations](managing-conversations.md) — Control conversation history
