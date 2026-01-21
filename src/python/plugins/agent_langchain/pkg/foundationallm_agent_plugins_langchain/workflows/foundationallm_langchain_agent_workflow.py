"""
Class: FoundationaLLMLangChainAgentWorkflow
Description: FoundationaLLM agent workflow based on the built-in LangChain ReAct Agent.
"""

import time
from dataclasses import dataclass
from typing import Dict, List, Optional
from opentelemetry.trace import SpanKind

from langchain_core.messages import (
    AIMessage,
    BaseMessage,
    HumanMessage,
    ToolMessage
)
from langchain.agents import create_agent

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

@dataclass
class Context:
    original_user_prompt: str
    recursion_limit: int

class FoundationaLLMLangChainAgentWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow based on the LangChain built-in ReAct Agent.
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
        Initializes the FoundationaLLMLangChainAgentWorkflow class with the workflow configuration.

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
        is_new_conversation: bool = False,
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

        message_list = self.__get_message_list(
            llm_prompt,
            message_history,
            objects
        )

        graph = create_agent(
            model=self.workflow_llm,
            tools=self.tools,
            system_prompt=workflow_main_prompt,
            context_schema=Context
        )

        # This is a port of the previous graph recursion limit handling
        # TODO: Clarify if this still has an effect with the new LangGraph implementation
        graph_recursion_limit = self.workflow_config.properties.get('graph_recursion_limit', None) if self.workflow_config.properties else None

        response = await graph.ainvoke(
            { "messages": message_list },
            context=Context(
                original_user_prompt=llm_prompt,
                recursion_limit=graph_recursion_limit))

        # TODO: process tool messages with analysis results AIMessage with content='' but has addition_kwargs={'tool_calls';[...]}

        # Get ContentArtifact items from ToolMessages
        tool_messages = [message for message in response["messages"] if isinstance(message, ToolMessage)]
        for tool_message in tool_messages:
            if tool_message.artifact is not None:
                # if the tool message artifact is a list, check if it contains a ContentArtifact item
                if isinstance(tool_message.artifact, list):
                    for item in tool_message.artifact:
                        if isinstance(item, ContentArtifact):
                            content_artifacts.append(item)

        final_message = response["messages"][-1]
        response_content = OpenAITextMessageContentItem(
            value = final_message.content,
            agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
        )

        workflow_end_time = time.time()
        output_tokens = final_message.usage_metadata["output_tokens"] or 0
        input_tokens = final_message.usage_metadata["input_tokens"] or 0


        workflow_content_artifact = self.create_workflow_execution_content_artifact(
                llm_prompt,
                input_tokens,
                output_tokens,
                workflow_end_time - workflow_start_time)
        content_artifacts.append(workflow_content_artifact)

        retvalue = CompletionResponse(
            operation_id=operation_id,
            content = [response_content],
            content_artifacts=content_artifacts,
            user_prompt=llm_prompt,
            full_prompt=workflow_main_prompt,
            completion_tokens=output_tokens,
            prompt_tokens=input_tokens,
            total_tokens=output_tokens + input_tokens,
            total_cost=0
        )

        if is_new_conversation:
            # Generate a conversation name if this is a new conversation.
            conversation_name, input_tokens, output_tokens = await self.get_conversation_name(
                llm_prompt,
                response_content.value
            )
            if conversation_name:
                retvalue.conversation_name = conversation_name
                retvalue.prompt_tokens += input_tokens
                retvalue.completion_tokens += output_tokens

        return retvalue

    def __get_message_list(
        self,
        llm_prompt: str,
        message_history: List[MessageHistoryItem],
        objects: dict
    ) -> List[BaseMessage]:
        """
        Returns the message history in the format required by the workflow.

        Parameters
        ----------
        llm_prompt : str
            The LLM prompt to be processed.
        message_history : List[MessageHistoryItem]
            The message history to be processed.
        objects : dict
            The exploded objects assigned from the agent. This is used to pass additional context to the workflow.
        """

        if objects is None:
            objects = {}

        # Convert message history to LangChain message types
        messages = []
        for message in message_history:
            # Convert MessageHistoryItem to appropriate LangChain message type
            if message.sender == "User":
                messages.append(HumanMessage(content=message.text))
            else:
                messages.append(AIMessage(content=message.text))

        return [
            *messages,
            HumanMessage(content=llm_prompt)
        ]
