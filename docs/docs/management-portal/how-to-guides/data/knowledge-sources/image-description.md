# Image Description

Learn about LLM-generated image description capabilities for processing visual content in FoundationaLLM.

## Overview

FoundationaLLM leverages Large Language Models (LLMs) with vision capabilities to process images, extract textual content, and generate rich descriptions. This makes visual content searchable and accessible to agents, enabling knowledge retrieval from image-based documents.

## LLM-Generated Image Descriptions

FoundationaLLM uses vision-capable LLMs to analyze images and generate detailed textual descriptions. These descriptions:

- Are generated using models like GPT-4 Vision or Claude Vision
- Can describe image content up to the model's context window limits
- Are stored as searchable text in your knowledge base
- Enable semantic search across visual content

### Description Quality Factors

| Factor | Impact |
|--------|--------|
| **Model Size** | Larger models produce more detailed, accurate descriptions |
| **Token Allocation** | More tokens allow longer, richer descriptions |
| **Image Resolution** | Higher resolution enables finer detail recognition |
| **Image Complexity** | Simple images are described more accurately |

## Capabilities

| Capability | Description |
|------------|-------------|
| **OCR (Optical Character Recognition)** | Extract text visible in images |
| **LLM Image Description** | Generate natural language descriptions using vision models |
| **Visual Q&A** | Answer questions about image content |
| **Content Summarization** | Create concise summaries of complex visual content |

## Supported Image Formats

| Format | Extension | Notes |
|--------|-----------|-------|
| **JPEG** | .jpg, .jpeg | Most common photo format |
| **PNG** | .png | Supports transparency |
| **GIF** | .gif | Static images only |
| **BMP** | .bmp | Uncompressed bitmap |
| **TIFF** | .tiff, .tif | High-quality images |
| **WebP** | .webp | Modern web format |

## Use Cases

### Document Processing

- **Scanned Documents**: Extract text from scanned PDFs and images
- **Forms**: Process filled-out forms and extract field values
- **Receipts/Invoices**: Digitize paper documents

### Visual Content Indexing

- **Diagrams**: Make technical diagrams searchable
- **Charts**: Extract data from chart images
- **Screenshots**: Index screenshot content

### Accessibility

- **Alt Text Generation**: Create descriptions for accessibility
- **Image Cataloging**: Describe and categorize image libraries

## Configuration

### Prerequisites

- A vision-capable AI model configured (e.g., GPT-4 Vision, Claude Vision)
- Data pipeline with image processing stages

### Data Pipeline Configuration

To process images in a data pipeline:

1. **Create or Edit a Data Pipeline**
2. **Select a Text Extraction Plugin** that supports images
3. **Configure Vision Model** for image processing
4. **Set Quality Parameters**:

| Parameter | Description |
|-----------|-------------|
| **Detail Level** | Low, medium, or high detail extraction |
| **Max Tokens** | Token limit for generated descriptions |

### Stage Configuration

> **TODO**: Document specific image processing stage configuration options in data pipelines.

## Model Considerations

### Token Limits and Model Size

Image processing consumes tokens from your AI model allocation. The quality and length of generated descriptions depends on your model configuration:

| Model Tier | Typical Token Limit | Description Quality |
|------------|---------------------|---------------------|
| **GPT-4 Vision** | Up to 128K tokens | Highly detailed, comprehensive descriptions |
| **GPT-4o** | Up to 128K tokens | Fast, accurate descriptions |
| **Claude 3 Vision** | Up to 200K tokens | Extended context for complex images |

**Key considerations:**

- Higher detail levels use more tokens per image
- Large images may require resizing before processing
- Batch processing can optimize throughput
- Token limits apply to both input (image) and output (description)

### Model Capabilities

| Model | OCR | Description | Analysis | Max Image Size |
|-------|-----|-------------|----------|----------------|
| **GPT-4 Vision** | ✅ | ✅ | ✅ | 20MB |
| **GPT-4o** | ✅ | ✅ | ✅ | 20MB |
| **Claude 3 Vision** | ✅ | ✅ | ✅ | 20MB |

> **Note**: Actual limits depend on your specific model deployment configuration. Contact your administrator for deployment-specific limits.

### Resolution Considerations

| Resolution | Processing | Quality | Cost |
|------------|------------|---------|------|
| **Low** | Fast | Basic | Lower |
| **Medium** | Moderate | Good | Medium |
| **High** | Slower | Best | Higher |

## Best Practices

### Image Quality

- Use high-resolution images when text extraction is critical
- Ensure good contrast between text and background
- Avoid heavily compressed images

### Processing Optimization

- Batch similar images together
- Use appropriate detail levels for your use case
- Consider preprocessing (cropping, enhancement) for poor quality sources

### Content Organization

- Store images with meaningful filenames
- Group related images in folders
- Include metadata when available

## Integration with Agents

After processing, image content becomes available to agents through:

1. **Vector Search**: Descriptions are embedded and searchable
2. **Direct Analysis**: Vision-capable agents can analyze images directly
3. **Contextual Answers**: Agents can reference image content in responses

## Limitations

| Limitation | Description |
|------------|-------------|
| **Handwriting** | Variable quality depending on legibility |
| **Complex Layouts** | Tables and forms may need specialized processing |
| **Image Quality** | Poor quality reduces extraction accuracy |
| **Language Support** | OCR accuracy varies by language |

## Troubleshooting

### Poor OCR Results

- Check image quality and resolution
- Ensure text has good contrast
- Consider preprocessing to enhance clarity

### Missing Descriptions

- Verify vision model is properly configured
- Check pipeline stage configuration
- Review model token limits

### Processing Failures

- Check image format is supported
- Verify image isn't corrupted
- Review size limits for your deployment

## Related Topics

- [Creating Data Pipelines](../data-pipelines/creating-data-pipelines.md)
- [Data Sources](../data-sources.md)
- [AI Models Configuration](../../models-endpoints/ai-models.md)
