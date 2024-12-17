from typing import Any, Self, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import object_utils
from .agent_workflow_base import AgentWorkflowBase

class LangChainSimpleToolAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for a LangChain simple tool agent workflow.
    """
    type: Literal["langchain-simple-tool-workflow"] = "langchain-simple-tool-workflow"
       
    @staticmethod
    def from_object(obj: Any) -> Self:

        workflow: LangChainSimpleToolAgentWorkflow = None

        try:
            workflow = LangChainSimpleToolAgentWorkflow(**object_utils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The LangChain simple tool Agent Workflow object provided is invalid. {str(e)}", 400)
        
        if workflow is None:
            raise LangChainException("The LangChain simple tool Agent Workflow object provided is invalid.", 400)

        return workflow
