import json
from enum import Enum
from azure.identity import DefaultAzureCredential, get_bearer_token_provider
from langchain_core.callbacks import AsyncCallbackManagerForToolRun, CallbackManagerForToolRun
from langchain_core.tools import ToolException
from openai import AsyncAzureOpenAI
from pydantic import BaseModel, Field
from typing import Optional, Type

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
    AIModelResourceTypeNames,
    PromptResourceTypeNames
)

class DALLEImageGenerationToolQualityEnum(str, Enum):
    """ Enum for the quality parameter of the DALL-E image generation tool. """
    standard = "standard"
    hd = "hd"

class DALLEImageGenerationToolStyleEnum(str, Enum):
    """ Enum for the style parameter of the DALL-E image generation tool. """
    natural = "natural"
    vivid = "vivid"

class DALLEImageGenerationToolSizeEnum(str, Enum):
    """ Enum for the size parameter of the DALL-E image generation tool. """
    size1024x1024 = "1024x1024"
    size1792x1024 = "1792x1024"
    size1024x1792 = "1024x1792"

class DALLEImageGenerationToolInput(BaseModel):
    """ Input data model for the DALL-E image generation tool. """
    prompt: str = Field(description="Prompt for the DALL-E image generation tool.", example="A cat in the forest.")
    n: int = Field(description="Number of images to generate.", example=1, default=1)
    quality: DALLEImageGenerationToolQualityEnum = Field(description="Quality of the generated images.", default=DALLEImageGenerationToolQualityEnum.hd)
    style: DALLEImageGenerationToolStyleEnum = Field(description="Style of the generated images.", default=DALLEImageGenerationToolStyleEnum.natural)
    size: DALLEImageGenerationToolSizeEnum = Field(description="Size of the generated images.", default=DALLEImageGenerationToolSizeEnum.size1024x1024)

class DALLEImageGenerationTool(FoundationaLLMToolBase):
    """
    DALL-E image generation tool.
    Supports only Azure Identity authentication.
    """
    args_schema: Type[BaseModel] = DALLEImageGenerationToolInput

    def __init__(self, tool_config: AgentTool, objects: dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the DALLEImageGenerationTool class with the tool configuration,
            exploded objects collection, user identity, and platform configuration. """
        super().__init__(tool_config, objects, user_identity, config)

        ai_model_object_id = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        if ai_model_object_id is None:
            raise LangChainException("The tools's AI models requires a main_model.", 400)

        self.ai_model = ObjectUtils.get_object_by_id(ai_model_object_id.object_id, self.objects, AIModelBase)
        self.api_endpoint = ObjectUtils.get_object_by_id(self.ai_model.endpoint_object_id, self.objects, APIEndpointConfiguration)
        self.client = self._get_client()

    def _run(self,
            prompt: str,
            n: int = 1,
            quality: DALLEImageGenerationToolQualityEnum = DALLEImageGenerationToolQualityEnum.hd,
            style: DALLEImageGenerationToolStyleEnum = DALLEImageGenerationToolStyleEnum.natural,
            size: DALLEImageGenerationToolSizeEnum = DALLEImageGenerationToolSizeEnum.size1024x1024,
            run_manager: Optional[CallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")

    async def _arun(self,
            prompt: str,
            n: int = 1,
            quality: DALLEImageGenerationToolQualityEnum = DALLEImageGenerationToolQualityEnum.hd,
            style: DALLEImageGenerationToolStyleEnum = DALLEImageGenerationToolStyleEnum.natural,
            size: DALLEImageGenerationToolSizeEnum = DALLEImageGenerationToolSizeEnum.size1024x1024,
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None
            ) -> str:
        """
        Generate an image using the Azure OpenAI client.
        """
        print(f'Attempting to generate {n} images with a style of {style}, quality of {quality}, and a size of {size}.')
        try:
            result = await self.client.images.generate(
                model = self.ai_model.deployment_name,
                prompt = prompt,
                n = n,
                quality = quality,
                style = style,
                size = size
            )
            content_artifacts = [
                ContentArtifact(
                    id=self.tool_config.name,
                    title=self.tool_config.name,
                    source='tool',
                    filepath=image_data.url,
                    type='image',
                    metadata = {
                        'tool_name': self.tool_config.name,
                        'tool_input': prompt,
                        'revised_tool_input': image_data.revised_prompt
                    }
                )
                for image_data in result.data
                if image_data.revised_prompt and image_data.url
            ]
            return json.loads(result.model_dump_json()), content_artifacts
        except Exception as e:
            print(f'Image generation error code and message: {e.code}; {e}')
            # Specifically handle content policy violation errors.
            if e.code in ['contentFilter', 'content_policy_violation']:
                err = e.message[e.message.find("{"):e.message.rfind("}")+1]
                err_json = err.replace("'", '"')
                err_json = err_json.replace("True", "true").replace("False", "false")
                obj = json.loads(err_json)
                cfr = obj['error']['inner_error']['content_filter_results']
                filtered = [k for k, v in cfr.items() if v['filtered']]
                error_fmt = f"The image generation request resulted in a content policy violation for the following category: {', '.join(filtered)}"
                raise ToolException(error_fmt)
            elif e.code in ['invalidPayload', 'invalid_payload']:
                raise ToolException(f'The image generation request is invalid: {e.message}')
            else:
                raise ToolException(f"An {e.code} error occurred while attempting to generate the requested image: {e.message}")

    def _get_client(self):
        """
        Returns the an AsyncOpenAI client for DALL-E image generation.
        """
        scope = self.api_endpoint.authentication_parameters.get('scope', 'https://cognitiveservices.azure.com/.default')
        # Set up a Azure AD token provider.
        token_provider = get_bearer_token_provider(
            DefaultAzureCredential(exclude_environment_credential=True),
            scope
        )
        return AsyncAzureOpenAI(
            azure_endpoint = self.api_endpoint.url,
            api_version = self.api_endpoint.api_version,
            azure_ad_token_provider = token_provider
        )
