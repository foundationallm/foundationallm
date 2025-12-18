from typing import Any, Self, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import object_utils
from .agent_workflow_base import AgentWorkflowBase

class LangChainAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for a LangChain Agent workflow.
    """
    type: Literal["langchain-agent-workflow"] = "langchain-agent-workflow"


    @staticmethod
    def from_object(obj: Any) -> Self:

        workflow: LangChainAgentWorkflow = None

        try:
            workflow = LangChainAgentWorkflow(**object_utils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The LangChain Agent Workflow object provided is invalid. {str(e)}", 400)

        if workflow is None:
            raise LangChainException("The LangChain Agent Workflow object provided is invalid.", 400)

        return workflow
