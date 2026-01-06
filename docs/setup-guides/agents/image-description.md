# LLM-Generated Image Description

FoundationaLLM supports LLM-generated image descriptions, enabling agents to analyze and describe uploaded images as part of multimodal conversations.

## Overview

When users upload images to a conversation, agents with image description capabilities can:

- Analyze image content
- Generate detailed descriptions
- Answer questions about the image
- Extract text from images (OCR)
- Identify objects, people, and scenes

## How It Works

```
User                    Agent                      LLM
  |                       |                         |
  | Upload image          |                         |
  |---------------------->|                         |
  |                       | Encode image (base64)   |
  |                       |------------------------>|
  |                       |                         |
  |                       | Generate description    |
  |                       |<------------------------|
  |                       |                         |
  | Receive description   |                         |
  |<----------------------|                         |
```

## Supported Models

Image description requires a vision-capable model:

| Model | Provider | Capabilities |
|-------|----------|--------------|
| GPT-4o | Azure OpenAI | Full vision support |
| GPT-4o-mini | Azure OpenAI | Full vision support |
| Claude 3.5 Sonnet | Anthropic | Full vision support |
| Claude 3 Opus | Anthropic | Full vision support |
| Gemini Pro Vision | Google | Full vision support |

<!-- [TODO: Confirm all supported models with vision capabilities] -->

## Supported Image Formats

| Format | Extension | MIME Type |
|--------|-----------|-----------|
| JPEG | .jpg, .jpeg | image/jpeg |
| PNG | .png | image/png |
| GIF | .gif | image/gif |
| WebP | .webp | image/webp |

## Image Size Limits

| Limit | Value | Notes |
|-------|-------|-------|
| Maximum file size | <!-- [TODO: Document limit] --> | Per image |
| Maximum dimensions | <!-- [TODO: Document limit] --> | Width x Height |
| Maximum per message | 10 images | Multi-file upload |

<!-- [TODO: Confirm image size limits based on model constraints] -->

## Enabling Image Description

### Agent Configuration

To enable image description for an agent:

1. Configure the agent to use a vision-capable model
2. Enable file upload in agent settings
3. Image analysis is automatic when images are uploaded

### Management Portal

1. Navigate to **Agents** > [Your Agent]
2. In **Workflow** section, select a vision-capable model
3. In **Agent Configuration**, enable **Allow file upload**

### API Configuration

```json
{
  "orchestration_settings": {
    "orchestrator": "LangChain",
    "endpoint_configuration": {
      "provider": "microsoft",
      "operation_type": "chat"
    },
    "model_parameters": {
      "deployment_name": "gpt-4o"
    }
  },
  "properties": {
    "allow_file_upload": true
  }
}
```

## Using Image Description

### Uploading Images

Users can upload images by:

1. Clicking the attachment icon in chat
2. Selecting image file(s) from their device
3. Images appear as thumbnails in the message

### Asking About Images

After uploading, users can ask:

- "What's in this image?"
- "Describe the contents of this image"
- "Extract any text from this image"
- "What objects can you identify?"
- Specific questions about image content

### Example Interaction

**User**: [Uploads image of a chart]

**User**: "Can you describe this chart and summarize the key findings?"

**Agent**: "This appears to be a bar chart showing quarterly sales data for 2024. The chart shows:

- Q1: $1.2M (baseline)
- Q2: $1.5M (25% increase)
- Q3: $1.8M (20% increase)
- Q4: $2.1M (17% increase)

Key findings: There's consistent quarter-over-quarter growth, with Q2 showing the strongest relative increase. Total annual sales reached $6.6M."

## Token Consumption

Image analysis consumes additional tokens:

| Image Aspect | Token Impact |
|--------------|--------------|
| Base processing | Fixed tokens per image |
| Resolution | Higher resolution = more tokens |
| Detail level | Detailed analysis uses more tokens |

<!-- [TODO: Provide specific token estimates per image size] -->

### Optimizing Token Usage

1. **Resize before upload**: Smaller images use fewer tokens
2. **Crop to relevant area**: Remove unnecessary portions
3. **Use appropriate detail level**: Ask for summary vs. detailed analysis

## Limitations

### Current Limitations

| Limitation | Description |
|------------|-------------|
| Model dependency | Requires vision-capable model |
| Processing time | Larger images take longer |
| Context window | Images compete with text for context |
| Accuracy | May misidentify content |

### Known Issues

<!-- [TODO: Document any known issues with image description] -->

| Issue | Workaround |
|-------|------------|
| TBD | TBD |

## Security Considerations

1. **Sensitive content**: Images are processed by external AI services
2. **Retention**: Understand how images are stored and for how long
3. **Content moderation**: Inappropriate images may be blocked
4. **PII in images**: Be cautious about images containing personal information

## Troubleshooting

### Image Not Analyzed

| Issue | Cause | Solution |
|-------|-------|----------|
| No description generated | Model doesn't support vision | Switch to vision-capable model |
| Upload failed | File too large | Reduce image size |
| Format not supported | Unsupported file type | Convert to supported format |

### Poor Quality Descriptions

| Issue | Solution |
|-------|----------|
| Vague descriptions | Ask more specific questions |
| Missed details | Request detailed analysis |
| Incorrect identification | Provide context in your question |

## Best Practices

1. **Clear images**: Use well-lit, focused images
2. **Relevant cropping**: Focus on the area of interest
3. **Specific questions**: Ask targeted questions
4. **Verify accuracy**: Double-check important details
5. **Appropriate models**: Use models suited to your needs

## Related Topics

- [Agents and Workflows](agents_workflows.md) - Model configuration
- [Self-Service Agent Creation](../user-portal/self-service-agent-creation.md) - Creating agents with image capabilities
- [DALL-E Image Generation](agents_workflows.md#dalle3-image-generator) - Generating images (opposite direction)
