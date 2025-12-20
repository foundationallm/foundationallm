# FoundationaLLM Image Model Support Enhancement Plan

## Executive Summary

This document outlines a comprehensive plan to enhance FoundationaLLM to support **image generation models**, starting with Google Gemini's `gemini-3-pro-image-preview` (Nano Banana Pro). This model represents a new category of AI models that can:

1. Generate images from text prompts
2. Modify/edit existing images with natural language instructions
3. Apply style transfer using reference images
4. Return mixed text + image responses

The implementation will follow FoundationaLLM's existing architectural patterns while extending the platform to handle multimodal outputs natively.

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
│  │   • Image Generation Workflow (new)                                    │ │
│  │   • Multimodal Chat Workflow (enhanced)                                │ │
│  │   • Tool-based Image Generation (existing)                             │ │
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
│  │   ImageResponseProcessor (new)                                          │ │
│  │   • Extract base64 images from model response                          │ │
│  │   • Upload to blob storage                                              │ │
│  │   • Generate accessible URLs                                            │ │
│  │   • Create ImageFileMessageContentItem                                  │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 New AI Model Type: `multimodal-generation`

Instead of just `image-generation`, introduce a more flexible type that supports models capable of both text and image generation:

```python
class AIModelTypes(str, Enum):
    BASIC = "basic"
    EMBEDDING = "embedding"
    COMPLETION = "completion"
    IMAGE_GENERATION = "image-generation"
    MULTIMODAL_GENERATION = "multimodal-generation"  # NEW
```

### 3.3 Extended Model Parameters

```python
class MultimodalGenerationAIModel(AIModelBase):
    """AI model capable of multimodal generation (text + images)."""
    
    # Response modalities configuration
    response_modalities: List[str] = ["TEXT", "IMAGE"]  # or just ["IMAGE"]
    
    # Image generation configuration
    image_config: Optional[ImageGenerationConfig] = None
    
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

### Phase 1: Foundation (Week 1-2)
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

### Phase 2: Completion Flow Integration (Week 2-3)
**Goal**: Enable image generation through normal completion requests

4. **Create Image Generation Workflow**
   - New workflow: `FoundationaLLMImageGenerationWorkflow`
   - Handles text → image generation
   - Handles image → modified image (editing)
   - Handles image + reference → styled image (style transfer)

5. **Extend CompletionResponse Model**
   - Add support for `generated_images` field
   - Update `OpenAIImageFileMessageContentItem` for base64 data URLs
   - Add image metadata (prompt used, aspect ratio, etc.)

6. **Update Agent Configuration**
   - New workflow type: `ImageGeneration`
   - New capability: `image_generation_enabled`
   - Model selection for image generation

### Phase 3: UI & User Experience (Week 3-4)
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

### Phase 4: Tool Integration (Week 4-5)
**Goal**: Allow image generation as a tool within existing agents

9. **Create Gemini Image Generation Tool**
   - Similar to DALL-E tool but for Gemini
   - Support image editing via tool
   - Support style transfer via tool

10. **Enhance Existing Tools**
    - Allow tools to return generated images
    - Update tool artifact handling for images

### Phase 5: Testing & Documentation (Week 5-6)
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

### 5.3 Image Response Processor

**New File**: `src/python/plugins/core/pkg/foundationallm/services/image_response_processor.py`

```python
"""
Image Response Processor for handling multimodal model outputs.
"""

import base64
import uuid
from typing import List, Optional, Tuple
from pathlib import Path

from langchain_core.messages import AIMessage

from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager
from foundationallm.models.orchestration import (
    OpenAIImageFileMessageContentItem,
    ContentArtifact
)
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ContentArtifactTypeNames
)


class ImageResponseProcessor:
    """
    Processes multimodal responses containing generated images.
    Extracts images, uploads to storage, and creates response content items.
    """
    
    def __init__(
        self,
        config: Configuration,
        storage_account_name: str,
        container_name: str
    ):
        self.config = config
        self.storage_manager = BlobStorageManager(
            account_name=storage_account_name,
            container_name=container_name,
            authentication_type=config.get_value(
                'FoundationaLLM:ResourceProviders:Attachment:Storage:AuthenticationType'
            )
        )
    
    def extract_images_from_response(
        self,
        response: AIMessage,
        operation_id: str,
        conversation_id: Optional[str] = None
    ) -> Tuple[List[OpenAIImageFileMessageContentItem], List[ContentArtifact]]:
        """
        Extract images from a multimodal AI response.
        
        Parameters
        ----------
        response : AIMessage
            The response from the language model.
        operation_id : str
            The operation ID for tracking.
        conversation_id : Optional[str]
            The conversation ID for organizing stored images.
            
        Returns
        -------
        Tuple[List[OpenAIImageFileMessageContentItem], List[ContentArtifact]]
            Tuple of content items and artifacts for the images.
        """
        content_items = []
        content_artifacts = []
        
        # Handle different response formats
        if isinstance(response.content, list):
            for idx, block in enumerate(response.content):
                if isinstance(block, dict) and block.get("image_url"):
                    # Extract base64 from data URL
                    url = block["image_url"].get("url", "")
                    if url.startswith("data:"):
                        image_data = self._extract_base64_from_data_url(url)
                        if image_data:
                            content_type, base64_data = image_data
                            
                            # Upload to storage
                            file_name = f"{operation_id}_{idx}.{self._get_extension(content_type)}"
                            blob_path = self._upload_image(
                                base64_data,
                                file_name,
                                conversation_id
                            )
                            
                            # Create content item
                            content_item = OpenAIImageFileMessageContentItem(
                                file_id=blob_path,
                                file_url=self._get_blob_url(blob_path),
                                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_IMAGE_GENERATION
                            )
                            content_items.append(content_item)
                            
                            # Create artifact
                            artifact = ContentArtifact(
                                id=f"generated_image_{idx}",
                                title=f"Generated Image {idx + 1}",
                                filepath=blob_path,
                                source="image_generation",
                                type=ContentArtifactTypeNames.GENERATED_IMAGE,
                                metadata={
                                    "content_type": content_type,
                                    "operation_id": operation_id
                                }
                            )
                            content_artifacts.append(artifact)
        
        return content_items, content_artifacts
    
    def _extract_base64_from_data_url(self, data_url: str) -> Optional[Tuple[str, str]]:
        """Extract content type and base64 data from a data URL."""
        try:
            # Format: data:image/png;base64,AAAA...
            header, data = data_url.split(",", 1)
            content_type = header.split(":")[1].split(";")[0]
            return content_type, data
        except (ValueError, IndexError):
            return None
    
    def _upload_image(
        self,
        base64_data: str,
        file_name: str,
        conversation_id: Optional[str] = None
    ) -> str:
        """Upload base64 image data to blob storage."""
        image_bytes = base64.b64decode(base64_data)
        
        # Organize by conversation if provided
        path_prefix = f"generated-images/{conversation_id}/" if conversation_id else "generated-images/"
        blob_path = f"{path_prefix}{file_name}"
        
        self.storage_manager.write_file_content(
            blob_path,
            image_bytes
        )
        
        return blob_path
    
    def _get_blob_url(self, blob_path: str) -> str:
        """Get accessible URL for a blob."""
        return self.storage_manager.get_blob_url(blob_path)
    
    def _get_extension(self, content_type: str) -> str:
        """Get file extension from content type."""
        extensions = {
            "image/png": "png",
            "image/jpeg": "jpg",
            "image/webp": "webp",
            "image/gif": "gif"
        }
        return extensions.get(content_type, "png")
```

### 5.4 Image Generation Workflow

**New File**: `src/python/plugins/agent_langchain/pkg/foundationallm_agent_plugins_langchain/workflows/foundationallm_image_generation_workflow.py`

```python
"""
Class: FoundationaLLMImageGenerationWorkflow
Description: FoundationaLLM workflow for image generation using multimodal models.
"""

import base64
import time
from typing import Dict, List, Optional

from langchain_core.messages import (
    AIMessage,
    BaseMessage,
    HumanMessage,
    SystemMessage
)

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMWorkflowBase, FoundationaLLMToolBase
from foundationallm.models.agents import GenericAgentWorkflow, AgentWorkflowBase
from foundationallm.models.constants import AgentCapabilityCategories
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem,
    OpenAIImageFileMessageContentItem
)
from foundationallm.operations import OperationsManager
from foundationallm.services import ImageResponseProcessor


class FoundationaLLMImageGenerationWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow for image generation using multimodal models
    like Google Gemini's gemini-3-pro-image-preview.
    
    Supports:
    - Text to image generation
    - Image editing with text prompts
    - Style transfer with reference images
    """
    
    def __init__(
        self,
        workflow_config: GenericAgentWorkflow | AgentWorkflowBase,
        objects: Dict,
        tools: List[FoundationaLLMToolBase],
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration,
        intercept_http_calls: bool = False
    ):
        super().__init__(
            workflow_config, objects, tools, operations_manager, user_identity, config
        )
        self.name = workflow_config.name
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while generating the image.'
        ) if workflow_config.properties else 'An error occurred while generating the image.'
        
        # Create the image generation LLM
        self.create_workflow_llm(intercept_http_calls=intercept_http_calls)
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)
        
        # Initialize image response processor
        storage_account = config.get_value(
            'FoundationaLLM:ResourceProviders:Attachment:Storage:AccountName'
        )
        container = config.get_value(
            'FoundationaLLM:ResourceProviders:Attachment:Storage:ContainerName'
        )
        self.image_processor = ImageResponseProcessor(
            config, storage_account, container
        )
    
    async def invoke_async(
        self,
        operation_id: str,
        user_prompt: str,
        user_prompt_rewrite: Optional[str],
        message_history: List[MessageHistoryItem],
        file_history: List[FileHistoryItem],
        conversation_id: Optional[str] = None,
        objects: dict = None
    ) -> CompletionResponse:
        """
        Invokes the image generation workflow.
        """
        workflow_start_time = time.time()
        
        if objects is None:
            objects = {}
        
        content_artifacts: List[ContentArtifact] = []
        response_content = []
        input_tokens = 0
        output_tokens = 0
        error_message = None
        
        llm_prompt = user_prompt_rewrite or user_prompt
        workflow_main_prompt = self.create_workflow_main_prompt()
        
        # Build messages including any input images
        messages = await self._build_messages(
            llm_prompt,
            workflow_main_prompt,
            message_history,
            file_history,
            objects
        )
        
        try:
            # Invoke the multimodal model
            response: AIMessage = await self.workflow_llm.ainvoke(messages)
            
            # Extract any text response
            text_content = self._extract_text_content(response)
            if text_content:
                response_content.append(OpenAITextMessageContentItem(
                    value=text_content,
                    agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_IMAGE_GENERATION
                ))
            
            # Extract and process generated images
            image_items, image_artifacts = self.image_processor.extract_images_from_response(
                response,
                operation_id,
                conversation_id
            )
            response_content.extend(image_items)
            content_artifacts.extend(image_artifacts)
            
            # Get usage statistics
            if response.usage_metadata:
                input_tokens = response.usage_metadata.get("input_tokens", 0)
                output_tokens = response.usage_metadata.get("output_tokens", 0)
                
        except Exception as ex:
            self.logger.error('Error during image generation: %s', str(ex))
            error_message = str(ex)
            response_content.append(OpenAITextMessageContentItem(
                value=f"Failed to generate image: {self.default_error_message}",
                agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_IMAGE_GENERATION
            ))
        
        workflow_end_time = time.time()
        
        # Create workflow execution artifact
        workflow_artifact = self.create_workflow_execution_content_artifact(
            llm_prompt,
            input_tokens,
            output_tokens,
            workflow_end_time - workflow_start_time,
            error_message=error_message
        )
        content_artifacts.append(workflow_artifact)
        
        return CompletionResponse(
            operation_id=operation_id,
            content=response_content,
            content_artifacts=content_artifacts,
            user_prompt=llm_prompt,
            full_prompt=workflow_main_prompt,
            completion_tokens=output_tokens,
            prompt_tokens=input_tokens,
            total_tokens=output_tokens + input_tokens,
            total_cost=0,
            is_error=error_message is not None,
            errors=[error_message] if error_message else []
        )
    
    async def _build_messages(
        self,
        llm_prompt: str,
        system_prompt: str,
        message_history: List[MessageHistoryItem],
        file_history: List[FileHistoryItem],
        objects: dict
    ) -> List[BaseMessage]:
        """
        Build the message list for the model, including any input images.
        """
        messages = [SystemMessage(content=system_prompt)]
        
        # Add conversation history
        for message in message_history:
            if message.sender == "User":
                messages.append(HumanMessage(content=message.text))
            else:
                messages.append(AIMessage(content=message.text))
        
        # Build the current message content (may include images)
        current_content = []
        current_content.append({"type": "text", "text": llm_prompt})
        
        # Add any attached images from file history
        for file_item in file_history:
            if file_item.content_type.startswith("image/") and file_item.embed_content_in_request:
                image_base64 = await self._load_image_as_base64(file_item)
                if image_base64:
                    current_content.append({
                        "type": "image_url",
                        "image_url": f"data:{file_item.content_type};base64,{image_base64}"
                    })
        
        messages.append(HumanMessage(content=current_content))
        return messages
    
    async def _load_image_as_base64(self, file_item: FileHistoryItem) -> Optional[str]:
        """Load an image file and convert to base64."""
        # Implementation depends on file storage mechanism
        # This is a placeholder for the actual implementation
        pass
    
    def _extract_text_content(self, response: AIMessage) -> Optional[str]:
        """Extract text content from a multimodal response."""
        if isinstance(response.content, str):
            return response.content
        
        if isinstance(response.content, list):
            text_parts = []
            for block in response.content:
                if isinstance(block, str):
                    text_parts.append(block)
                elif isinstance(block, dict) and block.get("type") == "text":
                    text_parts.append(block.get("text", ""))
            return " ".join(text_parts) if text_parts else None
        
        return None
```

### 5.5 Gemini Image Generation Tool

**New File**: `src/python/plugins/core/pkg/foundationallm/langchain/tools/gemini_image_generation_tool.py`

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


class GeminiImageGenerationTool(FoundationaLLMToolBase):
    """
    Gemini image generation tool supporting:
    - Text-to-image generation
    - Image editing with text prompts
    - Style transfer with reference images
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
                content_artifact = ContentArtifact(
                    id=self.tool_config.name,
                    title="Generated Image",
                    source="gemini_image_generation",
                    filepath=None,  # Will be set by caller
                    type="image",
                    metadata={
                        "tool_name": self.tool_config.name,
                        "prompt": prompt,
                        "aspect_ratio": aspect_ratio,
                        "is_edit": source_image_base64 is not None,
                        "has_style_reference": reference_image_base64 is not None
                    }
                )
                return image_data, [content_artifact]
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

### 5.6 New Constants and Types

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

### 5.7 Updated Completion Response Model

**File Update**: `src/python/plugins/core/pkg/foundationallm/models/orchestration/openai_image_file_message_content_item.py`

```python
from pydantic import Field
from typing import Optional, Literal
from .message_content_item_base import MessageContentItemBase
from .message_content_item_types import MessageContentItemTypes

class OpenAIImageFileMessageContentItem(MessageContentItemBase):
    """An image file message content item."""

    type: Literal[MessageContentItemTypes.IMAGE_FILE] = MessageContentItemTypes.IMAGE_FILE
    file_id: Optional[str] = Field(None, description="The storage path/ID of the image file.")
    file_url: Optional[str] = Field(None, description="The accessible URL of the image file.")
    
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
  "type": "multimodal-generation",
  "name": "gemini-3-pro-image",
  "display_name": "Gemini 3 Pro Image",
  "description": "Google Gemini 3 Pro for image generation and editing",
  "endpoint_object_id": "/instances/{instanceId}/providers/FoundationaLLM.Configuration/apiEndpointConfigurations/gemini-image-endpoint",
  "deployment_name": "gemini-3-pro-image-preview",
  "model_parameters": {
    "temperature": 1.0,
    "response_modalities": ["IMAGE", "TEXT"],
    "image_config": {
      "aspect_ratio": "1:1"
    }
  }
}
```

### 6.3 Agent Workflow Configuration

Example agent workflow for image generation:

```json
{
  "type": "image-generation-workflow",
  "name": "gemini-image-workflow",
  "workflow_host": "LangChain",
  "package_name": "foundationallm_agent_plugins_langchain",
  "class_name": "FoundationaLLMImageGenerationWorkflow",
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

2. **Image Response Processor Tests**
   - Test base64 extraction from data URLs
   - Test image upload to blob storage
   - Test content item creation

3. **Image Generation Workflow Tests**
   - Test text-to-image generation flow
   - Test image editing flow
   - Test error handling

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
        """Test generating an image from a text prompt."""
        # Setup
        workflow = create_test_workflow()
        
        # Execute
        response = await workflow.invoke_async(
            operation_id="test-op-1",
            user_prompt="Generate a sunset over mountains",
            user_prompt_rewrite=None,
            message_history=[],
            file_history=[],
            conversation_id="test-conv-1"
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
| `src/python/plugins/core/pkg/foundationallm/services/image_response_processor.py` | Process multimodal responses |
| `src/python/plugins/agent_langchain/pkg/.../foundationallm_image_generation_workflow.py` | Image generation workflow |
| `src/python/plugins/core/pkg/foundationallm/langchain/tools/gemini_image_generation_tool.py` | Gemini image tool |
| `src/dotnet/Common/Models/ResourceProviders/AIModel/MultimodalGenerationAIModel.cs` | New AI model type |
| `tests/python/test_gemini_image_generation.py` | Unit/integration tests |

### Modified Files

| File | Changes |
|------|---------|
| `language_model_factory.py` | Add GOOGLE_GENAI provider, image config support |
| `language_model_provider.py` | Add GOOGLE_GENAI enum |
| `APIEndpointProviders.cs` | Add GOOGLE_GENAI constant |
| `AIModelTypes.cs` | Add multimodal-generation type |
| `agent_capability_categories.py` | Add image generation category |
| `content_artifact_type_names.py` | Add generated_image type |
| `openai_image_file_message_content_item.py` | Add new metadata fields |
| `requirements.txt` | Ensure langchain-google-genai version |

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
*Created: December 2024*  
*Author: FoundationaLLM Enhancement Team*
