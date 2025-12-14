# Monitoring Token Consumption

Understand and track token usage during your conversations with AI agents.

## What Are Tokens?

Tokens are the units of text that AI models process. When you send a message or receive a response, the text is broken into tokens for processing.

### Token Basics

- **1 token ≈ 4 characters** in English (roughly 3/4 of a word)
- Shorter words may be one token, longer words may be multiple tokens
- Punctuation and spaces also consume tokens
- Non-English text may use more tokens per character

### Examples

| Text | Approximate Tokens |
|------|-------------------|
| "Hello" | 1 token |
| "Hello, how are you today?" | 7 tokens |
| "The quick brown fox jumps over the lazy dog" | 10 tokens |
| A 500-word document | ~650-700 tokens |

## Why Token Tracking Matters

Understanding token usage helps you:

- **Manage costs**: Higher token usage may affect your organization's AI usage costs
- **Optimize prompts**: Write more efficient prompts to reduce token consumption
- **Understand limits**: Know when you're approaching conversation or request limits
- **Troubleshoot**: Large token counts may explain slower responses

## Viewing Token Information

Token display is a feature that must be enabled by your administrator. When enabled:

### Token Chip

Each message displays a **token chip** showing the token count:
- Located in the top-right corner of each message, next to the timestamp
- Shows "Tokens: [number]"
- **User messages**: Show prompt tokens (what you sent)
- **Agent responses**: Show completion tokens (what was generated)

### Token Chip Colors

The token chip appearance differs based on the message type:
- **Your messages**: Chip matches your message's accent color
- **Agent responses**: Chip matches the agent message's theme color

## When Token Display Is Hidden

You won't see token information if:
- Your administrator hasn't enabled token display
- The current agent doesn't have token display enabled
- The message is still being generated (tokens show after completion)

> **Note:** Even when not displayed, tokens are still being consumed. Contact your administrator if you need token visibility enabled.

## Understanding Token Counts

### Prompt Tokens (Your Messages)

Prompt tokens include:
- The text you typed
- Any system instructions the agent uses
- Context from previous messages in the conversation
- Information from uploaded files being referenced

### Completion Tokens (Agent Responses)

Completion tokens include:
- The agent's response text
- Any formatted content (tables, lists, code)
- Generated data or analysis

### Total Tokens

The total tokens for a single exchange equals:
- Prompt tokens + Completion tokens

For a conversation, total usage is the sum of all message token counts.

## Tips for Optimizing Token Usage

### Write Concise Prompts

Instead of:
> "I would like you to please help me understand what the quarterly sales numbers were for the last fiscal year, if you could be so kind as to summarize them for me."

Try:
> "Summarize Q1-Q4 sales from last fiscal year."

### Be Specific

Clear, specific questions reduce the need for follow-up clarifications:
- Specify the format you want (bullet points, table, paragraph)
- Mention the level of detail needed
- Include relevant constraints upfront

### Use Conversation History Wisely

- Reference previous responses instead of repeating information
- Start new conversations for unrelated topics
- The agent includes recent history, which adds to prompt tokens

### Break Down Complex Requests

For complex tasks:
1. Ask one question at a time
2. Build on previous answers
3. This often uses fewer total tokens than one massive request

## Token Limits

### Message Limits

Individual messages may have token limits:
- Very long prompts may be truncated
- Extremely long responses may be cut short
- Your administrator sets these limits

### Conversation Context Limits

The conversation history included in each request has limits:
- Older messages may not be included in context
- This is why agents may "forget" earlier parts of long conversations

### Rate Limits

Your organization may have:
- Requests per minute limits
- Tokens per minute limits
- Daily or monthly token quotas

## Troubleshooting

### Token Count Seems Too High

High token counts may be due to:
- Long conversation history being included
- Uploaded files adding context
- Complex formatting in the response
- The agent retrieving information from knowledge bases

### Token Count Shows Zero or Not Displaying

- Wait for the message to finish generating
- Token display may not be enabled (ask your administrator)
- There may have been an error calculating tokens

### Response Cut Off Mid-Sentence

The response may have hit a token limit:
- Try asking for a shorter response
- Request the information in parts
- Ask the agent to continue from where it stopped

### Slow Responses with High Token Counts

More tokens = longer processing time:
- Consider breaking the request into smaller parts
- Ask for summaries instead of full details
- Reduce the complexity of your request

## Checking Your Usage

If you need to monitor overall token usage:
- Contact your administrator for usage reports
- Ask about organization-wide dashboards
- Review any quotas or limits that apply to your account

## Related Topics

- [Managing Conversations](managing-conversations.md) — Control conversation length and context
- [Copying Messages](copying-prompts-results.md) — Extract content without regenerating
- [Rating Responses](rating-responses.md) — Provide feedback on responses
