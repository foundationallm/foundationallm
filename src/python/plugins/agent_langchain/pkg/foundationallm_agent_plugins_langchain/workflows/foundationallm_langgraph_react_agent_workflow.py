"""
Class: FoundationaLLMLangGraphReActAgentWorkflow
Description: FoundationaLLM agent workflow based on the LangChain LangGraph ReAct Agent.
"""

import time
from typing import Dict, List, Optional
from opentelemetry.trace import SpanKind

from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.config import (
    Configuration,
    UserIdentity
)
from foundationallm.models.agents import (
    GenericAgentWorkflow,
    AgentWorkflowBase
)
from foundationallm.models.constants import (
    AgentCapabilityCategories
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionRequestObjectKeys,
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem
)
from foundationallm.operations import OperationsManager


class FoundationaLLMLangGraphReActAgentWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow based on the LangGraph ReAct Agent.
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
        """
        Initializes the FoundationaLLMLangGraphReActAgentWorkflow class with the workflow configuration.

        Parameters
        ----------
        workflow_config : GenericAgentWorkflow | AgentWorkflowBase
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[FoundationaLLMToolBase]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        intercept_http_calls : bool, optional
            Whether to intercept HTTP calls made by the workflow, by default False.
        """
        super().__init__(workflow_config, objects, tools, operations_manager, user_identity, config)
        self.name = workflow_config.name
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'

        # Sets self.workflow_llm
        self.create_workflow_llm(intercept_http_calls=intercept_http_calls)
        self.instance_id = objects.get(CompletionRequestObjectKeys.INSTANCE_ID, None)

    async def invoke_async(
        self,
        operation_id: str,
        user_prompt:str,
        user_prompt_rewrite: Optional[str],
        message_history: List[MessageHistoryItem],
        file_history: List[FileHistoryItem],
        conversation_id: Optional[str] = None,
        objects: dict = None
    )-> CompletionResponse:

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

        workflow_start_time = time.time()

        if objects is None:
            objects = {}

        content_artifacts: List[ContentArtifact] = []
        input_tokens = 0
        output_tokens = 0

        llm_prompt = user_prompt_rewrite or user_prompt
        workflow_main_prompt = self.create_workflow_main_prompt()

        workflow_end_time = time.time()

        workflow_content_artifact = self.create_workflow_execution_content_artifact(
                llm_prompt,
                input_tokens,
                output_tokens,
                workflow_end_time - workflow_start_time)
        content_artifacts.append(workflow_content_artifact)

        response_content = []
        final_response_content = OpenAITextMessageContentItem(
            value= 'Welcome to the LangGraph ReAct Agent workflow',
            agent_capability_category=AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
        )
        response_content.append(final_response_content)

        retvalue = CompletionResponse(
                operation_id=operation_id,
                content = response_content,
                content_artifacts=content_artifacts,
                user_prompt=llm_prompt,
                full_prompt=workflow_main_prompt,
                completion_tokens=output_tokens,
                prompt_tokens=input_tokens,
                total_tokens=output_tokens + input_tokens,
                total_cost=0
            )
        return retvalue
