# FoundationaLLM Image Model Support Enhancement Plan

## Executive Summary

This document outlines a comprehensive plan to enhance FoundationaLLM to support **image generation models**, starting with Google Gemini's `gemini-3-pro-image-preview` (Nano Banana Pro). This model represents a new category of AI models that can:

1. Generate images from text prompts
2. Modify/edit existing images with natural language instructions
3. Apply style transfer using reference images
4. Return mixed text + image responses

The implementation will follow FoundationaLLM's existing architectural patterns while extending the platform to handle multimodal outputs natively.

### Key Architectural Decisions

Based on architectural review, the following key decisions have been made:

1. **Model Type**: All image generation models use the existing `completion` type. No distinction between `COMPLETION`, `IMAGE_GENERATION`, and `MULTIMODAL_GENERATION` - image generation is fundamentally a form of completion.

2. **Image Configuration**: Image configuration (aspect ratio, response modalities) is specified **per completion request**, not in model parameters. This allows flexibility to change settings with each request, with reasonable defaults defined.

3. **Image Processing**: Generated image processing is handled by the **Orchestration API using the Context API file service**, not by LangChain API or direct blob storage. This follows FoundationaLLM's standard approach for file handling.

4. **Workflow Integration**: No separate image generation workflow is created. Image generation functionality is **integrated into the existing `FoundationaLLMFunctionCallingWorkflow`**.

5. **Tool Location**: Image generation tools (e.g., Gemini image generation tool) are placed in the **`agent_core` plugin** (`src/python/plugins/agent_core`), not in the core plugin.

---

## Table of Contents

1. [Current Architecture Analysis](#1-current-architecture-analysis)
2. [Gap Analysis](#2-gap-analysis)
3. [Proposed Solution Architecture](#3-proposed-solution-architecture)
4. [Implementation Phases](#4-implementation-phases)
5. [Detailed Component Changes](#5-detailed-component-changes)
6. [Configuration & Deployment](#6-configuration--deployment)
7. [Testing Strategy](#7-testing-strategy)
8. [Security Considerations](#8-security-considerations)
9. [Future Considerations](#9-future-considerations)

---

## 1. Current Architecture Analysis

### 1.1 Existing Model Support

FoundationaLLM currently supports these language model providers:

| Provider | Constant | Integration |
|----------|----------|-------------|
| Azure AI | `AZUREAI` | `langchain-azure-ai` |
| Microsoft (Azure OpenAI) | `MICROSOFT` | `langchain-openai` |
| OpenAI | `OPENAI` | `langchain-openai` |
| Amazon Bedrock | `BEDROCK` | `langchain-aws` |
| Google Vertex AI | `VERTEXAI` | `langchain-google-genai` |
| Databricks | `DATABRICKS` | `databricks-langchain` |

### 1.2 Existing AI Model Types

```
src/dotnet/Common/Constants/ResourceProviders/AIModelTypes.cs
```

- `basic` - Base type
- `embedding` - For embeddings
- `completion` - For text completion/chat
- `image-generation` - For image generation (exists but limited)

### 1.3 Existing Image Handling

1. **DALL-E Tool** (`dalle_image_generation_tool.py`):
   - Implemented as a LangChain tool
   - Uses Azure OpenAI's images API
   - Returns images via `ContentArtifact` with URL references
   - Limited to generation only (no editing)

2. **Image Service** (`image_service.py`):
   - Performs image analysis using multimodal prompts
   - Generates images via Azure OpenAI
   - Used internally by workflows

3. **Completion Response Model**:
   - Supports `OpenAIImageFileMessageContentItem` for image content
   - UI renders images via blob URLs

### 1.4 Key Files Identified

| Component | File Path |
|-----------|-----------|
| Language Model Factory | `src/python/plugins/core/pkg/foundationallm/langchain/language_models/language_model_factory.py` |
| Completion Response | `src/python/plugins/core/pkg/foundationallm/models/orchestration/completion_response.py` |
| Image Content Item | `src/python/plugins/core/pkg/foundationallm/models/orchestration/openai_image_file_message_content_item.py` |
| DALL-E Tool | `src/python/plugins/core/pkg/foundationallm/langchain/tools/dalle_image_generation_tool.py` |
| Provider Constants | `src/dotnet/Common/Constants/Configuration/APIEndpointProviders.cs` |
| AI Model Types | `src/dotnet/Common/Constants/ResourceProviders/AIModelTypes.cs` |
| Operation Types | `src/python/plugins/core/pkg/foundationallm/models/operations/operation_types.py` |
| Workflow Base | `src/python/plugins/core/pkg/foundationallm/langchain/common/foundationallm_workflow_base.py` |

---

## 2. Gap Analysis

### 2.1 Missing Capabilities

| Capability | Current State | Required State |
|------------|---------------|----------------|
| Native image generation from chat | Via tool only | Direct model output |
| Image editing/modification | Not supported | Supported |
| Style transfer with reference images | Not supported | Supported |
| Google Gemini Developer API | Not supported | Supported |
| Response modality control | Not supported | Supported |
| Image configuration (aspect ratio, etc.) | Not supported | Supported |
| Base64 image in response | Not handled | Handled |
| Multi-image input | Limited | Full support |

### 2.2 Missing Provider Support

The current `LanguageModelProvider` enum includes `VERTEXAI` but doesn't distinguish between:
- Google Vertex AI backend (enterprise, GCP-hosted)
- Google Gemini Developer API (simpler, API key-based)

Gemini's image generation model (`gemini-3-pro-image-preview`) can be accessed via both backends.

### 2.3 Model Parameter Gaps

Current `AIModelBase.model_parameters` doesn't support:
- `response_modalities` (e.g., `["TEXT", "IMAGE"]` or `["IMAGE"]`)
- `image_config` (e.g., `{"aspect_ratio": "16:9"}`)
- `temperature` specific to image generation
- Safety settings for image content

---

## 3. Proposed Solution Architecture

### 3.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           User Request Layer                                 │
│  ┌─────────────┐    ┌──────────────────┐    ┌─────────────────────────────┐ │
│  │  Text Only  │    │ Text + Image(s)  │    │    Image Edit Request       │ │
│  └──────┬──────┘    └────────┬─────────┘    └──────────────┬──────────────┘ │
└─────────┼────────────────────┼─────────────────────────────┼────────────────┘
          │                    │                             │
          ▼                    ▼                             ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Orchestration Layer                                   │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                    Agent / Workflow Selection                          │ │
│  │   • FoundationaLLMFunctionCallingWorkflow (enhanced)                   │ │
│  │     - Integrated image generation support                              │ │
│  │     - Direct model output for images                                   │ │
│  │   • Tool-based Image Generation (existing + new Gemini tool)          │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Language Model Factory Layer                             │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │   Extended LanguageModelFactory                                         │ │
│  │   • New: GOOGLE_GENAI provider support                                 │ │
│  │   • New: Image model configuration (response_modalities, image_config)  │ │
│  │   • New: ImageGenerationLanguageModel wrapper                          │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Provider Integration Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────────┐  │
│  │  Google Gemini  │  │  Azure OpenAI   │  │      Future Providers       │  │
│  │  Developer API  │  │  (DALL-E 3+)    │  │  (Midjourney, Stability AI) │  │
│  └─────────────────┘  └─────────────────┘  └─────────────────────────────┘  │
└───────────────────────────────┬─────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Response Processing Layer                            │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │   Orchestration API (via Context API File Service)                      │ │
│  │   • Extract base64 images from model response                          │ │
│  │   • Upload to Context API file service                                  │ │
│  │   • Generate accessible URLs via Context API                            │ │
│  │   • Create ImageFileMessageContentItem                                  │ │
│  │   • Note: Processing handled by Orchestration API, not LangChain API   │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 AI Model Type: Use Existing `completion` Type

**Important**: All image generation models should use the existing `completion` type. Image generation is fundamentally a form of completion, so no new model types are needed.

```python
class AIModelTypes(str, Enum):
    BASIC = "basic"
    EMBEDDING = "embedding"
    COMPLETION = "completion"  # Used for both text and image generation
    IMAGE_GENERATION = "image-generation"  # Existing, but new models should use COMPLETION
```

### 3.3 Image Configuration via Completion Request

Image configuration should be specified **per completion request**, not in model parameters. This allows flexibility to change image settings (aspect ratio, etc.) with each request. Reasonable defaults should be defined.

```python
class CompletionRequestBase(BaseModel):
    """Base class for completion requests."""
    operation_id: str
    session_id: Optional[str] = None
    user_prompt: str
    user_prompt_rewrite: Optional[str] = None
    message_history: Optional[List[MessageHistoryItem]] = []
    file_history: Optional[List[FileHistoryItem]] = []
    attachments: Optional[List[AttachmentProperties]] = []
    # NEW: Image generation configuration (optional, per-request)
    image_config: Optional[ImageGenerationConfig] = None

class ImageGenerationConfig(BaseModel):
    """Configuration for image generation in completion requests."""
    aspect_ratio: Optional[str] = "1:1"  # Default: square
    response_modalities: Optional[List[str]] = ["IMAGE"]  # Default: image only
    # Additional provider-specific options can be added here
```
    
    # Input modalities supported
    input_modalities: List[str] = ["TEXT", "IMAGE"]  # Can accept images as input
    
class ImageGenerationConfig(BaseModel):
    """Configuration for image generation."""
    aspect_ratio: Optional[str] = None  # "16:9", "4:3", "1:1", etc.
    output_format: Optional[str] = "png"  # "png", "jpeg", "webp"
    quality: Optional[str] = None  # Provider-specific
    style: Optional[str] = None  # Provider-specific
```

---

## 4. Implementation Phases

### Phase 1: Foundation
**Goal**: Establish core infrastructure for multimodal model support

1. **Add Google Gemini Developer API Provider**
   - Add `GOOGLE_GENAI` to `LanguageModelProvider` enum
   - Update `APIEndpointProviders.cs` for C# side
   - Add API key authentication support for Gemini

2. **Extend Language Model Factory**
   - Add `GOOGLE_GENAI` case in factory
   - Support `response_modalities` parameter
   - Support `image_config` parameter

3. **Add Base64 Image Response Handler**
   - Create `ImageResponseProcessor` service
   - Handle extraction of base64 images from responses
   - Upload images to blob storage
   - Return accessible URLs

### Phase 2: Completion Flow Integration
**Goal**: Enable image generation through normal completion requests

4. **Extend FoundationaLLMFunctionCallingWorkflow**
   - Integrate image generation support into existing function calling workflow
   - Handle image generation requests when model supports it
   - Process image responses from multimodal models
   - No separate image generation workflow needed

5. **Extend CompletionRequest Model**
   - Add optional `image_config` field to `CompletionRequestBase`
   - Support per-request image configuration (aspect ratio, response modalities)
   - Define reasonable defaults for image generation

6. **Extend CompletionResponse Model**
   - Update `OpenAIImageFileMessageContentItem` for base64 data URLs
   - Add image metadata (prompt used, aspect ratio, etc.)
   - Support mixed text + image responses

### Phase 3: UI & User Experience
**Goal**: Enable users to generate and view images in the chat interface

7. **Update User Portal**
   - Display generated images inline
   - Download functionality for generated images
   - Image editing UI (upload image + prompt)
   - Style reference upload support

8. **Update Management Portal**
   - Configure image generation agents
   - Select image generation models
   - Set default image parameters

### Phase 4: Tool Integration
**Goal**: Allow image generation as a tool within existing agents

9. **Create Gemini Image Generation Tool**
   - **Location**: `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/`
   - Similar to DALL-E tool but for Gemini
   - Support image editing via tool
   - Support style transfer via tool
   - Must be placed in agent_core plugin, not core plugin

10. **Enhance Existing Tools**
    - Allow tools to return generated images
    - Update tool artifact handling for images
    - Ensure tools use Context API for file storage

### Phase 5: Testing & Documentation
**Goal**: Ensure quality and provide guidance

11. **Testing**
    - Unit tests for new components
    - Integration tests for full flow
    - E2E tests in test harness

12. **Documentation**
    - API documentation updates
    - Configuration guides
    - User guides for image generation

---

## 5. Detailed Component Changes

### 5.1 New Provider: `GOOGLE_GENAI`

**File**: `src/python/plugins/core/pkg/foundationallm/models/language_models/language_model_provider.py`

```python
class LanguageModelProvider(str, Enum):
    """Enumerator of the Language Model providers."""
    AZUREAI = "azureai"
    MICROSOFT = "microsoft"
    OPENAI = "openai"
    BEDROCK = "bedrock"
    VERTEXAI = "vertexai"
    DATABRICKS = "databricks"
    GOOGLE_GENAI = "google_genai"  # NEW: Google Gemini Developer API
```

**File**: `src/dotnet/Common/Constants/Configuration/APIEndpointProviders.cs`

```csharp
public static class APIEndpointProviders
{
    // ... existing providers ...
    
    /// <summary>
    /// Google Gemini Developer API
    /// </summary>
    public const string GOOGLE_GENAI = "google_genai";
    
    public readonly static string[] All = [AZUREAI, MICROSOFT, OPENAI, BEDROCK, VERTEXAI, GOOGLE_GENAI];
}
```

### 5.2 Extended Language Model Factory

**File**: `src/python/plugins/core/pkg/foundationallm/langchain/language_models/language_model_factory.py`

```python
# New imports
from langchain_google_genai import ChatGoogleGenerativeAI, Modality

class LanguageModelFactory:
    
    def get_language_model(
        self,
        ai_model_object_id: str,
        override_operation_type: OperationTypes = None,
        agent_model_parameter_overrides: dict = None,
        http_async_client = None
    ) -> BaseLanguageModel:
        # ... existing code ...
        
        match api_endpoint.provider:
            # ... existing cases ...
            
            case LanguageModelProvider.GOOGLE_GENAI:
                # Google Gemini Developer API (API key based)
                try:
                    api_key = self.config.get_value(
                        api_endpoint.authentication_parameters.get('api_key_configuration_name')
                    )
                except Exception as e:
                    raise LangChainException(f"Failed to retrieve Gemini API key: {str(e)}", 500)
                
                if api_key is None:
                    raise LangChainException("Gemini API key is missing from configuration.", 400)
                
                # Build kwargs for ChatGoogleGenerativeAI
                model_kwargs = {
                    "model": ai_model.deployment_name,
                    "google_api_key": api_key,
                    "temperature": ai_model.model_parameters.get("temperature", 1.0),
                }
                
                # Handle image generation specific parameters
                if ai_model.model_parameters.get("response_modalities"):
                    modalities = []
                    for mod in ai_model.model_parameters["response_modalities"]:
                        if mod.upper() == "IMAGE":
                            modalities.append(Modality.IMAGE)
                        elif mod.upper() == "TEXT":
                            modalities.append(Modality.TEXT)
                    model_kwargs["response_modalities"] = modalities
                
                if ai_model.model_parameters.get("image_config"):
                    model_kwargs["image_config"] = ai_model.model_parameters["image_config"]
                
                language_model = ChatGoogleGenerativeAI(**model_kwargs)
```

### 5.3 Image Processing via Context API

**Important**: Image processing must be handled by the Orchestration API using the Context API file service, not by LangChain API or direct blob storage.

**Integration in FoundationaLLMFunctionCallingWorkflow**:

The existing `FoundationaLLMFunctionCallingWorkflow` already has access to the Context API client (`self.context_api_client`). When processing image responses:

1. Extract base64 images from model response
2. Use `context_api_client.post_async()` to create files via Context API
3. Context API handles storage and returns file records with URLs
4. Create `OpenAIImageFileMessageContentItem` with file references

**Example integration in workflow**:

```python
# In FoundationaLLMFunctionCallingWorkflow.invoke_async()

# After receiving LLM response with images
if isinstance(llm_response.content, list):
    for block in llm_response.content:
        if isinstance(block, dict) and block.get("image_url"):
            url = block["image_url"].get("url", "")
            if url.startswith("data:"):
                # Extract base64
                content_type, base64_data = extract_base64_from_data_url(url)
                image_bytes = base64.b64decode(base64_data)
                
                # Upload via Context API
                file_name = f"generated_image_{operation_id}_{idx}.png"
                self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
                
                context_file_response = await self.context_api_client.post_async(
                    endpoint=f"/instances/{self.instance_id}/files",
                    data={
                        "agent_name": objects.get('Agent.AgentName'),
                        "conversation_id": conversation_id,
                        "file_name": file_name,
                        "file_content_type": content_type,
                        "file_content": base64_data  # Context API expects base64
                    }
                )
                
                # Create content item from Context API response
                file_record = context_file_response.data
                content_items.append(OpenAIImageFileMessageContentItem(
                    file_id=file_record.object_id,
                    file_url=file_record.url,
                    agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_IMAGE_GENERATION
                ))
```

### 5.4 Enhanced Function Calling Workflow

**Modified File**: `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/workflows/foundationallm_function_calling_workflow.py`

**Note**: No separate image generation workflow is needed. Image generation functionality is integrated into the existing `FoundationaLLMFunctionCallingWorkflow`.

**Key Changes to FoundationaLLMFunctionCallingWorkflow**:

1. **Handle Image Configuration from Request**:
   - Read `image_config` from completion request objects
   - Pass image configuration to language model factory when creating LLM
   - Apply per-request image settings (aspect ratio, response modalities)

2. **Process Image Responses**:
   - Detect image content in LLM responses
   - Extract base64 images from multimodal responses
   - Upload images via Context API file service (not direct blob storage)
   - Create `OpenAIImageFileMessageContentItem` from Context API file records

3. **Support Mixed Responses**:
   - Handle responses containing both text and images
   - Properly format content items for UI display

**Example modifications**:

```python
# In FoundationaLLMFunctionCallingWorkflow.invoke_async()

# Extract image config from request
image_config = objects.get(CompletionRequestObjectKeys.IMAGE_CONFIG, None)

# When creating workflow LLM, pass image config if model supports it
# (This would be handled in create_workflow_llm or language_model_factory)

# After receiving LLM response
if isinstance(llm_response.content, list):
    for block in llm_response.content:
        if isinstance(block, dict) and block.get("image_url"):
            # Process image via Context API (see section 5.3)
            ...
```

### 5.5 Gemini Image Generation Tool

**New File**: `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_gemini_image_generation_tool.py`

**Important**: This tool must be placed in the `agent_core` plugin, not in the `core` plugin.

```python
"""
Gemini Image Generation Tool for FoundationaLLM.
Supports text-to-image, image editing, and style transfer.
"""

import base64
import json
from enum import Enum
from typing import Optional, Type, List

from langchain_core.callbacks import AsyncCallbackManagerForToolRun, CallbackManagerForToolRun
from langchain_core.tools import ToolException
from langchain_google_genai import ChatGoogleGenerativeAI, Modality
from pydantic import BaseModel, Field

from foundationallm.langchain.common import FoundationaLLMToolBase
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.agents import AgentTool
from foundationallm.models.orchestration import ContentArtifact
from foundationallm.models.resource_providers.ai_models import AIModelBase
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.constants import (
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    AIModelResourceTypeNames
)


class GeminiImageAspectRatioEnum(str, Enum):
    """Supported aspect ratios for Gemini image generation."""
    SQUARE = "1:1"
    LANDSCAPE = "16:9"
    PORTRAIT = "9:16"
    WIDE = "4:3"
    TALL = "3:4"


class GeminiImageGenerationToolInput(BaseModel):
    """Input model for the Gemini image generation tool."""
    prompt: str = Field(
        description="The text prompt describing the image to generate.",
        example="A futuristic city with flying cars at sunset"
    )
    aspect_ratio: GeminiImageAspectRatioEnum = Field(
        default=GeminiImageAspectRatioEnum.SQUARE,
        description="The aspect ratio of the generated image."
    )
    # For image editing
    source_image_base64: Optional[str] = Field(
        default=None,
        description="Base64-encoded source image for editing. Leave empty for new generation."
    )
    # For style transfer
    reference_image_base64: Optional[str] = Field(
        default=None,
        description="Base64-encoded reference image for style transfer."
    )


class FoundationaLLMGeminiImageGenerationTool(FoundationaLLMToolBase):
    """
    Gemini image generation tool supporting:
    - Text-to-image generation
    - Image editing with text prompts
    - Style transfer with reference images
    
    This tool is part of the agent_core plugin and uses Context API for file storage.
    """
    
    args_schema: Type[BaseModel] = GeminiImageGenerationToolInput
    
    def __init__(
        self,
        tool_config: AgentTool,
        objects: dict,
        user_identity: UserIdentity,
        config: Configuration
    ):
        super().__init__(tool_config, objects, user_identity, config)
        
        ai_model_object_id = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        if ai_model_object_id is None:
            raise LangChainException("The tool's AI model requires a main_model.", 400)
        
        self.ai_model = ObjectUtils.get_object_by_id(
            ai_model_object_id.object_id, self.objects, AIModelBase
        )
        self.api_endpoint = ObjectUtils.get_object_by_id(
            self.ai_model.endpoint_object_id, self.objects, APIEndpointConfiguration
        )
        self.client = self._get_client()
        
        # Initialize Context API client for file storage
        self.__create_context_client()
    
    def _run(
        self,
        prompt: str,
        aspect_ratio: GeminiImageAspectRatioEnum = GeminiImageAspectRatioEnum.SQUARE,
        source_image_base64: Optional[str] = None,
        reference_image_base64: Optional[str] = None,
        run_manager: Optional[CallbackManagerForToolRun] = None
    ) -> str:
        raise ToolException(
            "This tool does not support synchronous execution. Use the async version."
        )
    
    async def _arun(
        self,
        prompt: str,
        aspect_ratio: GeminiImageAspectRatioEnum = GeminiImageAspectRatioEnum.SQUARE,
        source_image_base64: Optional[str] = None,
        reference_image_base64: Optional[str] = None,
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None
    ) -> str:
        """Generate or edit an image using Gemini."""
        
        try:
            # Build the message content
            content = []
            
            # Add text prompt
            content.append({"type": "text", "text": prompt})
            
            # Add source image if editing
            if source_image_base64:
                content.append({
                    "type": "image_url",
                    "image_url": f"data:image/png;base64,{source_image_base64}"
                })
            
            # Add reference image for style transfer
            if reference_image_base64:
                content.append({
                    "type": "image_url", 
                    "image_url": f"data:image/png;base64,{reference_image_base64}"
                })
            
            # Invoke the model
            response = await self.client.ainvoke([
                {"role": "user", "content": content}
            ])
            
            # Extract the generated image
            image_data = self._extract_image(response)
            
            if image_data:
                # Upload image via Context API
                file_name = f"gemini_generated_{uuid.uuid4()}.png"
                self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
                
                context_file_response = await self.context_api_client.post_async(
                    endpoint=f"/instances/{self.instance_id}/files",
                    data={
                        "agent_name": self.objects.get('Agent.AgentName'),
                        "conversation_id": self.runnable_config.get(RunnableConfigKeys.CONVERSATION_ID),
                        "file_name": file_name,
                        "file_content_type": "image/png",
                        "file_content": image_data
                    }
                )
                
                file_record = context_file_response.data
                content_artifact = ContentArtifact(
                    id=self.tool_config.name,
                    title="Generated Image",
                    source="gemini_image_generation",
                    filepath=file_record.object_id,  # Context API file ID
                    type="image",
                    metadata={
                        "tool_name": self.tool_config.name,
                        "prompt": prompt,
                        "aspect_ratio": aspect_ratio,
                        "is_edit": source_image_base64 is not None,
                        "has_style_reference": reference_image_base64 is not None,
                        "file_url": file_record.url
                    }
                )
                return f"Image generated and saved: {file_record.url}", [content_artifact]
            else:
                raise ToolException("No image was generated in the response.")
                
        except Exception as e:
            error_msg = f"Image generation failed: {str(e)}"
            raise ToolException(error_msg)
    
    def _get_client(self) -> ChatGoogleGenerativeAI:
        """Create the Gemini client configured for image generation."""
        api_key = self.config.get_value(
            self.api_endpoint.authentication_parameters.get('api_key_configuration_name')
        )
        
        return ChatGoogleGenerativeAI(
            model=self.ai_model.deployment_name,
            google_api_key=api_key,
            temperature=1.0,
            response_modalities=[Modality.IMAGE],
            image_config={"aspect_ratio": "1:1"}  # Will be overridden per-request
        )
    
    def _extract_image(self, response) -> Optional[str]:
        """Extract base64 image data from the response."""
        if hasattr(response, 'content') and isinstance(response.content, list):
            for block in response.content:
                if isinstance(block, dict) and block.get("image_url"):
                    url = block["image_url"].get("url", "")
                    if url.startswith("data:"):
                        return url.split(",", 1)[-1]
        return None
```

### 5.6 Extended Completion Request Model

**File Update**: `src/python/plugins/core/pkg/foundationallm/models/orchestration/completion_request_base.py`

```python
from typing import List, Optional
from pydantic import BaseModel, Field
from foundationallm.models.attachments import AttachmentProperties
from foundationallm.models.messages import MessageHistoryItem
from .file_history_item import FileHistoryItem

class ImageGenerationConfig(BaseModel):
    """Configuration for image generation in completion requests."""
    aspect_ratio: Optional[str] = Field("1:1", description="Aspect ratio (e.g., '1:1', '16:9', '9:16')")
    response_modalities: Optional[List[str]] = Field(["IMAGE"], description="Response modalities: ['IMAGE'], ['TEXT', 'IMAGE'], etc.")

class CompletionRequestBase(BaseModel):
    """
    Base class for completion requests.
    """
    operation_id: str = Field(description="The operation ID for the completion request.")
    session_id: Optional[str] = Field(None, description="The session ID for the completion request.")
    user_prompt: str = Field(description="The user prompt for the completion request.")
    user_prompt_rewrite: Optional[str] = Field(None, description="The user prompt rewrite for the completion request.")
    message_history: Optional[List[MessageHistoryItem]] = Field(list, description="The message history for the completion.")
    file_history: Optional[List[FileHistoryItem]] = Field(list, description="The file history for the completion request.")
    attachments: Optional[List[AttachmentProperties]] = Field(list, description="The attachments collection for the completion request.")
    # NEW: Optional image generation configuration
    image_config: Optional[ImageGenerationConfig] = Field(None, description="Image generation configuration for this request.")
```

### 5.7 New Constants and Types

**File Update**: `src/python/plugins/core/pkg/foundationallm/models/constants/agent_capability_categories.py`

```python
class AgentCapabilityCategories:
    """Agent Capability Category Constants"""
    FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT = "FoundationaLLM.KnowledgeManagement"
    FOUNDATIONALLM_IMAGE_GENERATION = "FoundationaLLM.ImageGeneration"  # NEW
    FOUNDATIONALLM_AUDIO_ANALYSIS = "FoundationaLLM.AudioAnalysis"
    # ... other categories
```

**File Update**: `src/python/plugins/core/pkg/foundationallm/models/constants/content_artifact_type_names.py`

```python
class ContentArtifactTypeNames:
    """Content Artifact Type Constants"""
    WORKFLOW_EXECUTION = "workflow_execution"
    FILE = "file"
    GENERATED_IMAGE = "generated_image"  # NEW
    # ... other types
```

### 5.8 Updated Completion Response Model

**File Update**: `src/python/plugins/core/pkg/foundationallm/models/orchestration/openai_image_file_message_content_item.py`

```python
from pydantic import Field
from typing import Optional, Literal
from .message_content_item_base import MessageContentItemBase
from .message_content_item_types import MessageContentItemTypes

class OpenAIImageFileMessageContentItem(MessageContentItemBase):
    """An image file message content item."""

    type: Literal[MessageContentItemTypes.IMAGE_FILE] = MessageContentItemTypes.IMAGE_FILE
    file_id: Optional[str] = Field(None, description="The storage path/ID of the image file (Context API file ID).")
    file_url: Optional[str] = Field(None, description="The accessible URL of the image file (from Context API).")
    
    # New fields for richer metadata
    original_prompt: Optional[str] = Field(None, description="The prompt used to generate this image.")
    aspect_ratio: Optional[str] = Field(None, description="The aspect ratio of the image.")
    generation_model: Optional[str] = Field(None, description="The model used to generate this image.")
```

---

## 6. Configuration & Deployment

### 6.1 API Endpoint Configuration

Example configuration for Gemini image generation:

```json
{
  "type": "api-endpoint-configuration",
  "name": "gemini-image-endpoint",
  "display_name": "Gemini Image Generation Endpoint",
  "category": "image-generation",
  "provider": "google_genai",
  "authentication_type": "api-key",
  "authentication_parameters": {
    "api_key_configuration_name": "FoundationaLLM:APIEndpoints:GeminiImageEndpoint:APIKey"
  },
  "url": "https://generativelanguage.googleapis.com",
  "timeout_seconds": 120,
  "retry_strategy_name": "default"
}
```

### 6.2 AI Model Configuration

Example AI model configuration:

```json
{
  "type": "completion",
  "name": "gemini-3-pro-image",
  "display_name": "Gemini 3 Pro Image",
  "description": "Google Gemini 3 Pro for image generation and editing",
  "endpoint_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/gemini-image-endpoint",
  "deployment_name": "gemini-3-pro-image-preview",
  "model_parameters": {
    "temperature": 1.0
  }
}
```

**Note**: Image configuration (aspect ratio, response modalities) is specified per completion request, not in model configuration. The model type is `completion`, not `multimodal-generation` or `image-generation`.

### 6.3 Agent Workflow Configuration

Example agent workflow for image generation:

```json
{
  "type": "function-calling-workflow",
  "name": "gemini-image-workflow",
  "workflow_host": "FoundationaLLM",
  "package_name": "foundationallm_agent_plugins",
  "class_name": "FoundationaLLMFunctionCallingWorkflow",
  "resource_object_ids": {
    "/instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/gemini-3-pro-image": {
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.AIModel/aiModels/gemini-3-pro-image",
      "properties": {
        "object_role": "main_model"
      }
    },
    "/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/image-generation-prompt": {
      "object_id": "/instances/{instanceId}/providers/FoundationaLLM.Prompt/prompts/image-generation-prompt",
      "properties": {
        "object_role": "main_prompt"
      }
    }
  },
  "properties": {
    "default_error_message": "Sorry, I couldn't generate the requested image. Please try again with a different prompt."
  }
}
```

**Note**: Use the existing `FoundationaLLMFunctionCallingWorkflow`, not a separate image generation workflow.

### 6.4 Environment Variables

Required environment variables for Google Gemini:

```bash
# Gemini Developer API
GOOGLE_API_KEY=your_gemini_api_key

# Or via App Configuration
FoundationaLLM:APIEndpoints:GeminiImageEndpoint:APIKey=your_gemini_api_key

# Optional: Use Vertex AI backend instead
GOOGLE_GENAI_USE_VERTEXAI=true
GOOGLE_CLOUD_PROJECT=your-gcp-project-id
GOOGLE_CLOUD_LOCATION=us-central1
```

---

## 7. Testing Strategy

### 7.1 Unit Tests

**Test Coverage Areas:**

1. **Language Model Factory Tests**
   - Test GOOGLE_GENAI provider creation
   - Test response_modalities parameter handling
   - Test image_config parameter handling

2. **Function Calling Workflow Image Tests**
   - Test image generation in function calling workflow
   - Test image processing via Context API
   - Test per-request image configuration
   - Test mixed text + image responses

3. **Context API Integration Tests**
   - Test image upload via Context API file service
   - Test file record retrieval
   - Test URL generation

4. **Gemini Image Tool Tests**
   - Test tool input validation
   - Test successful generation
   - Test error scenarios

### 7.2 Integration Tests

```python
# tests/python/test_gemini_image_generation.py

import pytest
from unittest.mock import AsyncMock, patch

class TestGeminiImageGeneration:
    
    @pytest.mark.asyncio
    async def test_text_to_image_generation(self):
        """Test generating an image from a text prompt via function calling workflow."""
        # Setup
        workflow = create_test_function_calling_workflow()
        
        # Execute with image config
        image_config = ImageGenerationConfig(
            aspect_ratio="16:9",
            response_modalities=["IMAGE"]
        )
        response = await workflow.invoke_async(
            operation_id="test-op-1",
            user_prompt="Generate a sunset over mountains",
            user_prompt_rewrite=None,
            message_history=[],
            file_history=[],
            conversation_id="test-conv-1",
            objects={"ImageConfig": image_config}
        )
        
        # Assert
        assert len(response.content) > 0
        assert any(c.type == "image_file" for c in response.content)
    
    @pytest.mark.asyncio
    async def test_image_editing(self):
        """Test editing an existing image with a prompt."""
        # Test implementation
        pass
    
    @pytest.mark.asyncio
    async def test_style_transfer(self):
        """Test applying style from a reference image."""
        # Test implementation
        pass
```

### 7.3 E2E Test Harness Tests

Add to `evaluation/python/agent-test-harness/test-suites/`:

```json
{
  "name": "Image Generation Tests",
  "description": "End-to-end tests for image generation capabilities",
  "tests": [
    {
      "name": "Basic Image Generation",
      "prompt": "Generate an image of a cat wearing a top hat",
      "expected_response_type": "image",
      "validation": {
        "has_image": true,
        "image_count": 1
      }
    },
    {
      "name": "Image with Specific Aspect Ratio",
      "prompt": "Create a landscape image of a beach sunset in 16:9 aspect ratio",
      "expected_response_type": "image",
      "validation": {
        "has_image": true,
        "aspect_ratio": "16:9"
      }
    }
  ]
}
```

---

## 8. Security Considerations

### 8.1 Content Safety

1. **Input Validation**
   - Validate prompts for prohibited content before sending to model
   - Implement content filtering for harmful/inappropriate requests
   - Log and audit all image generation requests

2. **Output Filtering**
   - Review generated images for policy violations
   - Implement automated content moderation if available
   - Provide option to enable/disable adult content filtering

3. **Rate Limiting**
   - Implement per-user rate limits for image generation
   - Monitor for abuse patterns
   - Cost tracking for image generation operations

### 8.2 Data Privacy

1. **Image Storage**
   - Encrypt images at rest in blob storage
   - Implement appropriate retention policies
   - Support user data deletion requests

2. **Input Images**
   - Don't log raw input images (only metadata)
   - Clear temporary image data after processing
   - Respect user privacy for uploaded reference images

### 8.3 Access Control

1. **Feature Gating**
   - Make image generation a configurable capability per agent
   - Role-based access to image generation features
   - Audit trail for all image operations

---

## 9. Future Considerations

### 9.1 Additional Providers

Plan for future integration with:

- **Midjourney** (when API becomes available)
- **Stability AI** (Stable Diffusion)
- **Adobe Firefly**
- **DALL-E 4** (future versions)

### 9.2 Advanced Features

- **Inpainting/Outpainting**: Edit specific regions of images
- **Image Upscaling**: Enhance image resolution
- **Image Variations**: Generate variations of existing images
- **Batch Generation**: Generate multiple images at once
- **Video Generation**: Extend to video models (Sora, Runway, etc.)

### 9.3 UI Enhancements

- **Image Editor**: In-browser image editing before/after generation
- **Prompt Builder**: Visual prompt building tools
- **Gallery View**: Browse and manage generated images
- **Comparison View**: Compare original vs edited images

### 9.4 Workflow Enhancements

- **Image-in-loop**: Use generated images as context for subsequent prompts
- **Multi-step Generation**: Chain multiple generation steps
- **Conditional Generation**: Generate based on complex criteria

---

## Appendix A: File Change Summary

### New Files

| File | Description |
|------|-------------|
| `src/python/plugins/agent_core/pkg/foundationallm_agent_plugins/tools/foundationallm_gemini_image_generation_tool.py` | Gemini image generation tool (in agent_core plugin) |
| `tests/python/test_gemini_image_generation.py` | Unit/integration tests |

### Modified Files

| File | Changes |
|------|---------|
| `language_model_factory.py` | Add GOOGLE_GENAI provider, support image config from request |
| `language_model_provider.py` | Add GOOGLE_GENAI enum |
| `APIEndpointProviders.cs` | Add GOOGLE_GENAI constant |
| `completion_request_base.py` | Add optional `image_config` field |
| `foundationallm_function_calling_workflow.py` | Integrate image generation support, use Context API for file storage |
| `agent_capability_categories.py` | Add image generation category |
| `content_artifact_type_names.py` | Add generated_image type |
| `openai_image_file_message_content_item.py` | Add new metadata fields |
| `requirements.txt` | Ensure langchain-google-genai version |
| `tool_plugin_manager.py` (agent_core) | Register Gemini image generation tool |

---

## Appendix B: Dependencies

### Python Dependencies

```txt
# Already present, ensure latest version
langchain-google-genai>=4.1.1

# May need to add
google-generativeai>=0.8.0
```

### Package Versions Tested

- `langchain-google-genai`: 4.1.1
- `langchain`: 1.0.3
- `google-generativeai`: 0.8.0

---

## Appendix C: API Reference

### Gemini Image Generation Response Format

```python
# Response from ChatGoogleGenerativeAI with image modality

response.content = [
    {
        "type": "text",
        "text": "Here's the image you requested..."
    },
    {
        "image_url": {
            "url": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA..."
        }
    }
]
```

### Helper Function: Extract Image

```python
def extract_image_base64(response: AIMessage) -> str:
    """
    Find the first image block in response.content and return the base64 payload.
    """
    for block in response.content:
        if isinstance(block, dict) and block.get("image_url"):
            url = block["image_url"].get("url")
            if url and url.startswith("data:"):
                return url.split(",", 1)[-1]
    return None
```

---

*Document Version: 1.0*  
*Author: FoundationaLLM Enhancement Team*
