# pylint: disable=W0221

import base64
import uuid
from enum import Enum
from typing import Optional, Tuple, Type

from pydantic import BaseModel, Field

from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableConfig
from langchain_core.tools import ToolException
from langchain_google_genai import ChatGoogleGenerativeAI, Modality

from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMToolBase,
    FoundationaLLMToolResult
)
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
    RunnableConfigKeys,
    AIModelResourceTypeNames
)
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    ContentArtifact
)
from foundationallm.models.resource_providers.ai_models import AIModelBase
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.services import HttpClientService
from foundationallm.utils import ObjectUtils


class GeminiImageAspectRatioEnum(str, Enum):
    """Supported aspect ratios for Gemini image generation."""
    SQUARE = "1:1"
    LANDSCAPE = "16:9"
    PORTRAIT = "9:16"
    WIDE = "4:3"
    TALL = "3:4"


class FoundationaLLMGeminiImageGenerationToolInput(BaseModel):
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
    
    This tool uses the Context API for file storage.
    """
    
    args_schema: Type[BaseModel] = FoundationaLLMGeminiImageGenerationToolInput
    
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
        self.context_api_client = self.get_context_api_client(user_identity, config)
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)
    
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
        run_manager: Optional[AsyncCallbackManagerForToolRun] = None,
        runnable_config: RunnableConfig = None
    ) -> Tuple[str, FoundationaLLMToolResult]:
        """Generate or edit an image using Gemini."""
        
        if runnable_config is None:
            raise ToolException("RunnableConfig is required for the execution of the tool.")
        
        conversation_id = None
        if RunnableConfigKeys.CONVERSATION_ID in runnable_config.get('configurable', {}):
            conversation_id = runnable_config['configurable'][RunnableConfigKeys.CONVERSATION_ID]
        
        agent_name = None
        if 'agent_name' in runnable_config.get('configurable', {}):
            agent_name = runnable_config['configurable']['agent_name']
        
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
            
            # Configure client for image generation
            image_config = {"aspect_ratio": aspect_ratio.value}
            self.client.image_config = image_config
            self.client.response_modalities = [Modality.IMAGE]
            
            # Invoke the model
            response = await self.client.ainvoke([
                {"role": "user", "content": content}
            ])
            
            # Extract the generated image
            image_data = self._extract_image(response)
            
            if image_data:
                # Upload image via Context API
                import json
                file_name = f"gemini_generated_{uuid.uuid4().hex[:8]}.png"
                self.context_api_client.headers['X-USER-IDENTITY'] = self.user_identity.model_dump_json()
                
                context_file_response = await self.context_api_client.post_async(
                    endpoint=f"/instances/{self.instance_id}/files",
                    data=json.dumps({
                        "agent_name": agent_name or "default",
                        "conversation_id": conversation_id,
                        "file_name": file_name,
                        "file_content_type": "image/png",
                        "file_content": image_data  # base64 string
                    })
                )
                
                if context_file_response and hasattr(context_file_response, 'data'):
                    file_record = context_file_response.data
                    file_id = file_record.object_id if hasattr(file_record, 'object_id') else file_record.get('object_id')
                    file_url = file_record.url if hasattr(file_record, 'url') else file_record.get('url')
                    
                    content_artifact = ContentArtifact(
                        id=self.tool_config.name,
                        title="Generated Image",
                        source="gemini_image_generation",
                        filepath=file_id,
                        type=ContentArtifactTypeNames.GENERATED_IMAGE,
                        metadata={
                            "tool_name": self.tool_config.name,
                            "prompt": prompt,
                            "aspect_ratio": aspect_ratio.value,
                            "is_edit": source_image_base64 is not None,
                            "has_style_reference": reference_image_base64 is not None,
                            "file_url": file_url
                        }
                    )
                    
                    result = FoundationaLLMToolResult(
                        content=f"Image generated and saved: {file_url}",
                        content_artifacts=[content_artifact],
                        input_tokens=0,
                        output_tokens=0
                    )
                    return f"Image generated successfully: {file_url}", result
                else:
                    raise ToolException("Failed to save generated image via Context API.")
            else:
                raise ToolException("No image was generated in the response.")
                
        except Exception as e:
            error_msg = f"Image generation failed: {str(e)}"
            self.logger.error(error_msg)
            raise ToolException(error_msg)
    
    def _get_client(self) -> ChatGoogleGenerativeAI:
        """Create the Gemini client configured for image generation."""
        api_key = self.config.get_value(
            self.api_endpoint.authentication_parameters.get('api_key_configuration_name')
        )
        
        if not api_key:
            raise LangChainException("Gemini API key is missing from configuration.", 400)
        
        return ChatGoogleGenerativeAI(
            model=self.ai_model.deployment_name,
            google_api_key=api_key,
            temperature=1.0,
            response_modalities=[Modality.IMAGE],
            image_config={"aspect_ratio": "1:1"}  # Default, will be overridden per-request
        )
    
    def _extract_image(self, response) -> Optional[str]:
        """Extract base64 image data from the response."""
        if hasattr(response, 'content') and isinstance(response.content, list):
            for block in response.content:
                if isinstance(block, dict) and block.get("image_url"):
                    url = block["image_url"].get("url", "")
                    if url.startswith("data:"):
                        # Extract base64 part (after the comma)
                        return url.split(",", 1)[-1]
        return None
