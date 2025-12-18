from pydantic import Field
from typing import Any, Self, Optional, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import object_utils
from .agent_workflow_base import AgentWorkflowBase

class LangGraphReactAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for a LangGraph ReAct agent workflow.
    """
    type: Literal["langgraph-react-agent-workflow"] = "langgraph-react-agent-workflow"
    graph_recursion_limit: Optional[int] = Field(None, alias="graph_recursion_limit")
   
    @staticmethod
    def from_object(obj: Any) -> Self:

        workflow: LangGraphReactAgentWorkflow = None

        try:
            workflow = LangGraphReactAgentWorkflow(**object_utils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The LangGraph ReAct Agent Workflow object provided is invalid. {str(e)}", 400)
        
        if workflow is None:
            raise LangChainException("The LangGraph ReAct Agent Workflow object provided is invalid.", 400)

        return workflow
