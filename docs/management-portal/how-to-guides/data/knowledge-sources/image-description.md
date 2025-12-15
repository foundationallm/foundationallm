# Image Description

Learn about image-to-text description capabilities for processing visual content in FoundationaLLM.

## Overview

FoundationaLLM can process images to extract textual content and generate descriptions, making visual content searchable and accessible to agents.

## Capabilities

| Capability | Description |
|------------|-------------|
| **OCR (Optical Character Recognition)** | Extract text visible in images |
| **Image Description** | Generate natural language descriptions of image content |
| **Visual Q&A** | Answer questions about image content |

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

### Token Limits

Image processing consumes tokens from your AI model allocation:

- Higher detail levels use more tokens
- Large images may require resizing
- Batch processing can optimize throughput

### Model Capabilities

| Model | OCR | Description | Analysis |
|-------|-----|-------------|----------|
| **GPT-4 Vision** | ✅ | ✅ | ✅ |
| **Claude Vision** | ✅ | ✅ | ✅ |

> **TODO**: Document specific model configurations supported in your deployment.

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
