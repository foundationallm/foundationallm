"""
Class: FoundationaLLMToolBase
Description: FoundationaLLM base class for tools that uses the AgentTool model for its configuration.
"""

from typing import List, Dict

# Platform imports
from azure.identity import DefaultAzureCredential
from logging import Logger
from opentelemetry.trace import Tracer

# LangChain imports
from langchain_core.language_models import BaseLanguageModel
from langchain_core.tools import BaseTool

# FoundationaLLM imports
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.agents import AgentTool
from foundationallm.models.constants import (
    AIModelResourceTypeNames,
    ContentArtifactTypeNames,
    PromptResourceTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames)
from foundationallm.models.orchestration import ContentArtifact
from foundationallm.telemetry import Telemetry

class FoundationaLLMToolBase(BaseTool):
    """
    FoundationaLLM base class for tools that uses the AgentTool model for its configuration.
    """

    response_format: str = 'content_and_artifact'

    def __init__(self, tool_config: AgentTool, objects:Dict, user_identity:UserIdentity, config: Configuration):
        """ Initializes the FoundationaLLMToolBase class with the tool configuration. """
        super().__init__(
            name=tool_config.name,
            description=tool_config.description
        )
        self.tool_config = tool_config
        self.objects = objects
        self.user_identity = user_identity
        self.config = config

        self.language_model_factory = LanguageModelFactory(objects, config)

        self.logger: Logger = Telemetry.get_logger(self.name)
        self.tracer: Tracer = Telemetry.get_tracer(self.name)
        self.default_credential = DefaultAzureCredential(exclude_environment_credential=True)

    def get_language_model(
        self,
        role: str,
        http_async_client=None
    ) -> BaseLanguageModel:
        """
        Creates a language model based on the resource object identifier with a specified role.
        """

        ai_model_definition = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            role)
        return self.language_model_factory.get_language_model(
            ai_model_definition.object_id,
            http_async_client=http_async_client
        )

    def get_main_language_model(
        self,
        http_async_client=None
    ) -> BaseLanguageModel:
        """
        Creates the main language model of the tool.
        The maine language model is specified by the resource object identifier with the role 'main_model'.
        """
        return self.get_language_model(
            ResourceObjectIdPropertyValues.MAIN_MODEL,
            http_async_client=http_async_client)

    def get_prompt(self, role: str) -> str:
        """
        Creates a prompt based on the resource object identifier with a specified role.
        """

        prompt_object_id = self.tool_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            role
        )

        if prompt_object_id:
            main_prompt_object_id = prompt_object_id.object_id
            main_prompt_properties = self.objects[main_prompt_object_id]
            main_prompt = main_prompt_properties['prefix']

            return main_prompt
        else:
            self.logger.warning("No prompt object identifier found for the specified role.")
            return None

    def get_main_prompt(self) -> str:
        """
        Creates the main prompt of the tool.
        The main prompt is specified by the resource object identifier with the role 'main_prompt'.
        """
        return self.get_prompt(ResourceObjectIdPropertyValues.MAIN_PROMPT)

    def create_content_artifact(
            self,
            original_prompt: str,
            title: str = None,
            tool_input: str = None,
            prompt_tokens: int = 0,
            completion_tokens : int = 0
    ) -> ContentArtifact:
        """
        Creates a tool execution artifact.
        """

        tool_artifact = ContentArtifact(id=self.name)
        tool_artifact.title = f'{self.name} - {title}' if title else self.name
        tool_artifact.source = self.name
        tool_artifact.type = ContentArtifactTypeNames.TOOL_EXECUTION
        tool_artifact.content = original_prompt
        tool_artifact.filepath = None
        tool_artifact.metadata = {
            'tool_input': tool_input,
            'prompt_tokens': str(prompt_tokens),
            'completion_tokens': str(completion_tokens)
        }

        return tool_artifact

    def create_error_content_artifact(
            self,
            original_prompt: str,
            e: Exception
    ) -> ContentArtifact:
        """
        Creates an error content artifact.
        """

        error_artifact = ContentArtifact(id=self.name)
        error_artifact.title = f'{self.name} - Error'
        error_artifact.source = self.name
        error_artifact.type = ContentArtifactTypeNames.TOOL_ERROR
        error_artifact.content = repr(e)
        error_artifact.metadata = {
            'tool': self.name,
            'error_message': str(e),
            'prompt': original_prompt
        }
        return error_artifact

    class Config:
        """ Pydantic configuration for FoundationaLLMToolBase. """
        extra = "allow"
