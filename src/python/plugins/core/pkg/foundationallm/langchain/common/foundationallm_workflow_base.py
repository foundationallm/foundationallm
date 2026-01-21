"""
Class: FoundationaLLMWorkflowBase
Description: FoundationaLLM base class for tools that uses the agent workflow model for its configuration.
"""

from abc import ABC, abstractmethod
from typing import Dict, List, Optional
from opentelemetry.trace import SpanKind

from azure.identity import DefaultAzureCredential
from langchain_core.messages import (
    AIMessage,
    BaseMessage,
    HumanMessage,
    SystemMessage
)

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

        self.name = workflow_config.name
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'
        
        self.workflow_llm = None  # To be set in derived classes by calling create_workflow_llm()

    @abstractmethod
    async def invoke_async(
        self,
        operation_id: str,
        user_prompt: str,
        user_prompt_rewrite: Optional[str],
        message_history: List[MessageHistoryItem],
        file_history: List[FileHistoryItem],
        conversation_id: Optional[str] = None,
        is_new_conversation: bool = False,
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
        """
        Creates a content artifact for workflow execution.

        Parameters
        ----------
        original_prompt : str
            The original prompt used in the workflow execution.
        input_tokens : int
            The number of input tokens used. Defaults to 0.
        output_tokens : int
            The number of output tokens generated. Defaults to 0.
        completion_time_seconds : float
            The time taken for completion in seconds. Defaults to 0.
        error_message : Optional[str]
            An optional error message if an error occurred.

        Returns
        -------
        ContentArtifact
            The content artifact containing workflow execution details.
        """
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
        """
        Creates the workflow LLM instance and saves it to self.workflow_llm.

        Parameters
        ----------
        intercept_http_calls : bool
            Whether to intercept HTTP calls for logging purposes. Defaults to False.

        Raises
        ------
        ValueError
            If no main model is found in the workflow configuration.
        """
        language_model_factory = LanguageModelFactory(self.objects, self.config)
        model_object_id = self.workflow_config.get_resource_object_id_properties(
            ResourceProviderNames.FOUNDATIONALLM_AIMODEL,
            AIModelResourceTypeNames.AI_MODELS,
            ResourceObjectIdPropertyNames.OBJECT_ROLE,
            ResourceObjectIdPropertyValues.MAIN_MODEL
        )
        if model_object_id:
            http_async_client = LoggingAsyncHttpClient(timeout=30.0) if intercept_http_calls else None
            model_parameter_overrides = model_object_id.properties.get('model_parameters', {})
            self.workflow_llm = \
                language_model_factory.get_language_model(
                    model_object_id.object_id,
                    agent_model_parameter_overrides=model_parameter_overrides,
                    http_async_client=http_async_client
                )
        else:
            error_msg = 'No main model found in workflow configuration'
            self.logger.error(error_msg)
            raise ValueError(error_msg)

    def create_workflow_main_prompt(self) -> str:
        """
        Creates the workflow main prompt.

        Returns
        -------
        str
            The main prompt prefix from the workflow configuration.

        Raises
        ------
        ValueError
            If no main prompt is found in the workflow configuration.
        """
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
        """
        Creates the workflow router prompt.

        Returns
        -------
        str
            The router prompt prefix from the workflow configuration.

        Raises
        ------
        ValueError
            If no router prompt is found in the workflow configuration.
        """
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
        """
        Creates the workflow files prompt.

        Returns
        -------
        str or None
            The files prompt prefix from the workflow configuration, or None if not found.
        """
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
        """
        Creates the workflow final prompt.

        Returns
        -------
        str or None
            The final prompt prefix from the workflow configuration, or None if not found.
        """
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
        """
        Gets the main model definition from the workflow configuration.

        Returns
        -------
        AIModelBase
            The AI model definition for the main model.
        """
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
        """
        Gets the API endpoint configuration for an AI model.

        Parameters
        ----------
        ai_model : AIModelBase
            The AI model to get the endpoint configuration for.

        Returns
        -------
        APIEndpointConfiguration
            The API endpoint configuration for the AI model.
        """
        api_endpoint = ObjectUtils.get_object_by_id(ai_model.endpoint_object_id, self.objects, APIEndpointConfiguration)
        return api_endpoint

    def get_text_from_message(self, message: BaseMessage) -> str:
        """
        Extracts text from content blocks returned by the LLM.

        Parameters
        ----------
        message : BaseMessage
            The message containing content blocks from the LLM response.

        Returns
        -------
        str
            The extracted text from all text-type content blocks, joined with spaces.
        """
        text_parts = [block["text"] for block in message.content_blocks if block.get("type") == "text"]
        text = " ".join(text_parts)
        return text.strip()

    def get_canonical_usage(
            self,
            llm_response: AIMessage
    ) -> Dict:
        """
        Returns the canonical usage dictionary from the LLM response.

        Parameters
        ----------
        llm_response : AIMessage
            The LLM response message containing usage metadata.
        """
        if llm_response.usage_metadata:
            return llm_response.usage_metadata

        if llm_response.response_metadata \
            and 'usage' in llm_response.response_metadata \
            and 'prompt_tokens' in llm_response.response_metadata['usage'] \
            and 'completion_tokens' in llm_response.response_metadata['usage']:
            return {
                'input_tokens': llm_response.response_metadata['usage']['prompt_tokens'],
                'output_tokens': llm_response.response_metadata['usage']['completion_tokens'],
                'total_tokens': llm_response.response_metadata['usage']['total_tokens']
            }

        return {
            'input_tokens': 0,
            'output_tokens': 0,
            'total_tokens': 0
        }

    async def get_conversation_name(
        self,
        user_prompt: str,
        agent_response: str
    ) -> tuple[str, int, int]:
        """
        Generates a conversation name based on the initial user message.

        Parameters
        ----------
        user_prompt: str
            The initial user message of the conversation.
        agent_response: str
            The agent's response to the user message.

        Returns
        -------
        tuple[str, int, int]
            A tuple containing the generated conversation name, input tokens, and output tokens.
        """
        input_tokens = 0
        output_tokens = 0
        conversation_name = "New Conversation"

        messages = [
            SystemMessage(content="Generate a brief title (3-6 words) that summarizes the main topic of this conversation. Return only the title, no quotes or punctuation."),
            HumanMessage(content=user_prompt),
            AIMessage(content=agent_response)
        ]

        with self.tracer.start_as_current_span(
            f'{self.name}_get_conversation_name',
            kind=SpanKind.INTERNAL
        ):
            try:
                llm_response = await self.workflow_llm.ainvoke(messages)
                conversation_name = self.get_text_from_message(llm_response)
                usage = self.get_canonical_usage(llm_response)
                input_tokens = usage['input_tokens']
                output_tokens = usage['output_tokens']
                return conversation_name, input_tokens, output_tokens
            except Exception as ex:
                self.logger.error('Error during conversation name generation: %s', str(ex))
                return None, 0, 0

