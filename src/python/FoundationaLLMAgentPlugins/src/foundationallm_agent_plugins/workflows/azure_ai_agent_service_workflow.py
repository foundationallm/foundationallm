"""
Class: AzureAIAgentServiceWorkflow
Description: Workflow that integrates with the Azure AI Agent Service.
"""
from typing import Dict, List, Optional
from logging import Logger
from opentelemetry.trace import Tracer
from langchain_core.runnables import RunnableConfig
from opentelemetry.trace import SpanKind
from foundationallm.config import Configuration, UserIdentity
from foundationallm.langchain.common import (
    FoundationaLLMWorkflowBase,
    FoundationaLLMToolBase
)
from foundationallm.models.agents import ExternalAgentWorkflow
from foundationallm.models.constants import (
    AgentCapabilityCategories,
    ContentArtifactTypeNames,
    ResourceObjectIdPropertyNames,
    ResourceObjectIdPropertyValues,
    ResourceProviderNames,   
    RunnableConfigKeys
)
from foundationallm.models.messages import MessageHistoryItem
from foundationallm.models.orchestration import (
    CompletionResponse,
    ContentArtifact,
    FileHistoryItem,
    OpenAITextMessageContentItem,
    OpenAIImageFileMessageContentItem,
    OpenAIFilePathMessageContentItem
)
from foundationallm.telemetry import Telemetry

class AzureAIAgentServiceWorkflow(FoundationaLLMWorkflowBase):
    """
    FoundationaLLM workflow implementing an integration with the Azure AI Agent Service.
    """

    def __init__(self,
                 workflow_config: ExternalAgentWorkflow,
                 objects: Dict,
                 tools: List[FoundationaLLMToolBase],
                 user_identity: UserIdentity,
                 config: Configuration):
        """
        Initializes the FoundationaLLMWorkflowBase class with the workflow configuration.

        Parameters
        ----------
        workflow_config : ExternalAgentWorkflow
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
        super().__init__(workflow_config, objects, tools, user_identity, config)
        self.name = workflow_config.name
        self.logger : Logger = Telemetry.get_logger(self.name)
        self.tracer : Tracer = Telemetry.get_tracer(self.name)
        self.default_error_message = workflow_config.properties.get(
            'default_error_message',
            'An error occurred while processing the request.') \
            if workflow_config.properties else 'An error occurred while processing the request.'

        # Create prompt first, then LLM
        self.__create_workflow_prompt()

    async def invoke_async(self,
                           operation_id: str,
                           user_prompt:str,
                           user_prompt_rewrite: Optional[str],
                           message_history: List[MessageHistoryItem],
                           file_history: List[FileHistoryItem])-> CompletionResponse:
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
        """
        llm_prompt = user_prompt_rewrite or user_prompt
        runnable_config = RunnableConfig(
            {
                RunnableConfigKeys.ORIGINAL_USER_PROMPT: user_prompt,
                RunnableConfigKeys.ORIGINAL_USER_PROMPT_REWRITE: user_prompt_rewrite
            }
        )       

 

        retvalue = CompletionResponse(
            operation_id=operation_id,
            content = "Under development",
            content_artifacts=[],
            user_prompt=llm_prompt,
            full_prompt=self.workflow_prompt,
            completion_tokens=0,
            prompt_tokens=0,
            total_tokens=0,
            total_cost=0
        )
        return retvalue    

    def __create_workflow_execution_content_artifact(
            self,
            original_prompt: str,            
            prompt_tokens: int = 0,
            completion_tokens: int = 0,
            completion_time_seconds: float = 0) -> ContentArtifact:

        content_artifact = ContentArtifact(id=self.workflow_config.name)
        content_artifact.source = self.workflow_config.name
        content_artifact.type = ContentArtifactTypeNames.WORKFLOW_EXECUTION
        content_artifact.content = original_prompt
        content_artifact.title = self.workflow_config.name
        content_artifact.filepath = None
        content_artifact.metadata = {            
            'prompt_tokens': str(prompt_tokens),
            'completion_tokens': str(completion_tokens),
            'completion_time_seconds': str(completion_time_seconds)
        }
        return content_artifact
