"""
Class: FoundationaLLMWorkflowBase
Description: FoundationaLLM base class for tools that uses the agent workflow model for its configuration.
"""

from abc import ABC, abstractmethod
from typing import List, Optional

from azure.identity import DefaultAzureCredential

from foundationallm.langchain.common import (
    FoundationaLLMToolBase
)
from foundationallm.config import (
    Configuration,
    UserIdentity
)
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.models.agents import (
    GenericAgentWorkflow,
    ExternalAgentWorkflow,
    AgentWorkflowBase
)
from foundationallm.models.constants import (
    AIModelResourceTypeNames,
    ContentArtifactTypeNames,
    PromptResourceTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem
)
from foundationallm.models.resource_providers.ai_models import AIModelBase
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.operations import OperationsManager
from foundationallm.telemetry import Telemetry
from foundationallm.utils import LoggingAsyncHttpClient, ObjectUtils

class FoundationaLLMWorkflowBase(ABC):
    """
    FoundationaLLM base class for workflows that uses the agent workflow model for its configuration.
    """
    def __init__(
        self,
        workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow | AgentWorkflowBase,
        objects: dict,
        tools: List[FoundationaLLMToolBase],
        operations_manager: OperationsManager,
        user_identity: UserIdentity,
        config: Configuration
    ):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : GenericAgentWorkflow | ExternalAgentWorkflow
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        """
        self.workflow_config = workflow_config
        self.objects = objects
        self.tools = tools if tools is not None else []
        self.operations_manager = operations_manager
        self.user_identity = user_identity
        self.config = config
        self.logger = Telemetry.get_logger(self.workflow_config.name)
        self.tracer = Telemetry.get_tracer(self.workflow_config.name)
        self.default_credential = DefaultAzureCredential(exclude_environment_credential=True)

    @abstractmethod
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
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        user_prompt_rewrite : str
            The user prompt rewrite message containing additional context to clarify the user's intent.
        message_history : List[BaseMessage]
            The message history.
        file_history : List[FileHistoryItem]
            The file history.
        conversation_id : Optional[str]
            The conversation identifier for the workflow execution.
        objects : dict
            The exploded objects assigned from the agent. This is used to pass additional context to the workflow.
        """

    def create_workflow_execution_content_artifact(
        self,
        original_prompt: str,
        input_tokens: int = 0,
        output_tokens: int = 0,
        completion_time_seconds: float = 0,
        error_message: Optional[str] = None
    ) -> ContentArtifact:

        content_artifact = ContentArtifact(id=self.workflow_config.name)
        content_artifact.source = self.workflow_config.name
        content_artifact.type = ContentArtifactTypeNames.WORKFLOW_EXECUTION
        content_artifact.content = original_prompt
        content_artifact.title = self.workflow_config.name
        content_artifact.filepath = None
        content_artifact.metadata = {
            'prompt_tokens': str(input_tokens),
            'completion_tokens': str(output_tokens),
            'completion_time_seconds': str(completion_time_seconds)
        }
        if error_message:
            content_artifact.metadata['error_message'] = error_message
        return content_artifact

    def create_workflow_llm(
            self,
            intercept_http_calls: bool = False):
        """ Creates the workflow LLM instance and saves it to self.workflow_llm. """
        language_model_factory = LanguageModelFactory(self.objects, self.config)
        model_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        if model_object_id:
            self.workflow_llm = \
                language_model_factory.get_language_model(
                    model_object_id.object_id,
                    http_async_client=LoggingAsyncHttpClient(timeout=30.0)
                ) if intercept_http_calls else \
                language_model_factory.get_language_model(
                    model_object_id.object_id
                )
        else:
            error_msg = 'No main model found in workflow configuration'
            self.logger.error(error_msg)
            raise ValueError(error_msg)

    def create_workflow_main_prompt(self) -> str:
        """ Creates the workflow main prompt. """
        prompt_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_PROMPT
        )
        if prompt_object_id:
            main_prompt_object_id = prompt_object_id.object_id
            main_prompt_properties = self.objects[main_prompt_object_id]
            return main_prompt_properties['prefix']
        else:
            error_message = 'No main prompt found in workflow configuration'
            self.logger.error(error_message)
            raise ValueError(error_message)

    def create_workflow_router_prompt(self) -> str:
        """ Creates the workflow router prompt. """
        prompt_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            'router_prompt'
            # ResourceObjectIdPropertyValues.ROUTER_PROMPT
        )
        if prompt_object_id:
            router_prompt_object_id = prompt_object_id.object_id
            router_prompt_properties = self.objects[router_prompt_object_id]
            return router_prompt_properties['prefix']
        else:
            error_message = 'No router prompt found in workflow configuration'
            self.logger.error(error_message)
            raise ValueError(error_message)

    def create_workflow_files_prompt(self) -> str:
        """ Creates the workflow files prompt. """
        files_prompt_properties = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.FILES_PROMPT
        )
        if files_prompt_properties:
            files_prompt_object_id = files_prompt_properties.object_id
            return \
                self.objects[files_prompt_object_id]['prefix'] if files_prompt_object_id in self.objects else None
        else:
            warning_message = 'No files prompt found in workflow configuration'
            self.logger.warning(warning_message)
            return None

    def create_workflow_final_prompt(self) -> str:
        """ Creates the workflow final prompt. """
        final_prompt_properties = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_PROMPT,
            PromptResourceTypeNames.PROMPTS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.FINAL_PROMPT
        )
        if final_prompt_properties:
            final_prompt_object_id = final_prompt_properties.object_id
            return \
                self.objects[final_prompt_object_id]['prefix'] if final_prompt_object_id in self.objects else None
        else:
            warning_message = 'No final prompt found in workflow configuration'
            self.logger.warning(warning_message)
            return None

    def get_workflow_main_model_definition(
        self
    ) -> AIModelBase:
        main_model_properties = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        main_model_object_id = main_model_properties.object_id
        ai_model = ObjectUtils.get_object_by_id(main_model_object_id, self.objects, AIModelBase)
        return ai_model

    def get_ai_model_api_endpoint_configuration(
        self,
        ai_model: AIModelBase
    ) -> APIEndpointConfiguration:
        api_endpoint = ObjectUtils.get_object_by_id(ai_model.endpoint_object_id, self.objects, APIEndpointConfiguration)
        return api_endpoint
