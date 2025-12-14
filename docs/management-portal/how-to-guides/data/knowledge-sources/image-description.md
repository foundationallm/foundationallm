# Image Description

Learn about image-to-text description capabilities in FoundationaLLM.

## Overview

FoundationaLLM can extract descriptions from images to make visual content searchable and usable by agents.

## Image-to-Text Description Capability

The platform can:
- Extract text from images (OCR)
- Generate descriptions of image content
- Make images searchable in knowledge bases

## Model Limits

Image processing capabilities depend on:
- Configured vision models
- Model token limits
- Image resolution constraints

## Supported Formats

- **JPEG/JPG**: Standard photo format
- **PNG**: Lossless images with transparency
- **GIF**: Static GIF images
- **BMP**: Bitmap images
- **TIFF**: High-quality images

## Usage Guidelines

### When to Use Image Description
- Document scanning and digitization
- Visual content indexing
- Accessibility improvements

### Best Practices
- Use high-quality images
- Ensure adequate lighting and contrast
- Consider image size and resolution

## Configuration

Configure image processing in your data pipeline:
1. Select a content text extraction plugin that supports images
2. Configure the vision model
3. Set quality and detail parameters

## Related Topics

- [Creating Data Pipelines](../data-pipelines/creating-data-pipelines.md)
- [Data Sources](../data-sources.md)
