from pydantic import Field
from typing import Any, Self, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import object_utils
from .agent_workflow_base import AgentWorkflowBase

class AzureOpenAIAssistantsAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for an Azure OpenAI Assistants agent workflow.
    """
    type: Literal["azure-openai-assistants-workflow"] = "azure-openai-assistants-workflow"
    assistant_id: str = Field(description="The ID of the assistant in the OpenAI Assistants API service.")
   
    @staticmethod
    def from_object(obj: Any) -> Self:

        workflow: AzureOpenAIAssistantsAgentWorkflow = None

        try:
            workflow = AzureOpenAIAssistantsAgentWorkflow(**object_utils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"TheAzure OpenAI Assistants agent workflow object provided is invalid. {str(e)}", 400)
        
        if workflow is None:
            raise LangChainException("The Azure OpenAI Assistants agent workflow object provided is invalid.", 400)

        return workflow
