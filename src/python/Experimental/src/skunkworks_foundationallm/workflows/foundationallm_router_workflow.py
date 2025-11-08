"""
Class: FoundationaLLMWorkflowBase
Description: FoundationaLLM base class for tools that uses the agent workflow model for its configuration.
"""
from abc import ABC, abstractmethod
from typing import List
from langchain_core.messages import BaseMessage
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import FoundationaLLMWorkflowBase
from foundationallm.models.agents import (
    AgentTool,
    GenericAgentWorkflow,
    ExternalAgentWorkflow
)
from foundationallm.models.constants import AgentCapabilityCategories
from foundationallm.models.orchestration import CompletionResponse, OpenAITextMessageContentItem
from foundationallm.operations import OperationsManager

class FoundationaLLMRouterWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM base class for workflows that uses the agent workflow model for its configuration.
    """
    def __init__(self,
                 workflow_config: GenericAgentWorkflow | ExternalAgentWorkflow,
                 objects: dict,
                 tools: List[AgentTool],
                 operations_manager: OperationsManager,
                 user_identity: UserIdentity,
                 config: Configuration):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : GenericAgentWorkflow | ExternalAgentWorkflow
            The workflow assigned to the agent.
        objects : dict
            The exploded objects assigned from the agent.
        tools : List[AgentTool]
            The tools assigned to the agent.
        user_identity : UserIdentity
            The user identity of the user initiating the request.
        config : Configuration
            The application configuration for FoundationaLLM.
        """
        super().__init__(workflow_config, objects, tools, operations_manager, user_identity, config)

    async def invoke_async(self,
                           operation_id: str,
                           user_prompt:str,
                           message_history: List[BaseMessage])-> CompletionResponse:
        """
        Invokes the workflow asynchronously.

        Parameters
        ----------
        operation_id : str
            The unique identifier of the FoundationaLLM operation.
        user_prompt : str
            The user prompt message.
        message_history : List[BaseMessage]
            The message history.
        """
        response_content = OpenAITextMessageContentItem(
            value = '42 is the answer to all questions',
            agent_capability_category = AgentCapabilityCategories.FOUNDATIONALLM_KNOWLEDGE_MANAGEMENT
        )


        return CompletionResponse(
            operation_id = operation_id,
            content = [response_content],
            content_artifacts = [],
            user_prompt = user_prompt,
            full_prompt = '',
            completion_tokens = 0,
            prompt_tokens = 0,
            total_tokens = 0,
            total_cost = 0
        )
