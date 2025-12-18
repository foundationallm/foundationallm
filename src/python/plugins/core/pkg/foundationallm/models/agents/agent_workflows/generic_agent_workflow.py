from typing import Any, Self, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import ObjectUtils
from .agent_workflow_base import AgentWorkflowBase

class GenericAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for a generic agent workflow implemented by a plugin.
    """
    type: Literal["generic-agent-workflow"] = "generic-agent-workflow"


    @staticmethod
    def from_object(obj: Any) -> Self:

        workflow: GenericAgentWorkflow = None

        try:
            workflow = GenericAgentWorkflow(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The Generic Agent Workflow object provided is invalid. {str(e)}", 400)

        if workflow is None:
            raise LangChainException("The Generic Agent Workflow object provided is invalid.", 400)

        return workflow
