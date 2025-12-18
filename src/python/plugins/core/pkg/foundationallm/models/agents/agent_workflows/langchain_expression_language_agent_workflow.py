from typing import Any, Self, Literal
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.utils import object_utils
from .agent_workflow_base import AgentWorkflowBase

class LangChainExpressionLanguageAgentWorkflow(AgentWorkflowBase):
    """
    The configuration for a LangChain Expression Language agent workflow.
    """
    type: Literal["langchain-expression-language-workflow"] = "langchain-expression-language-workflow"
    
   
    @staticmethod
    def from_object(obj: Any) -> Self:

        workflow: LangChainExpressionLanguageAgentWorkflow = None

        try:
            workflow = LangChainExpressionLanguageAgentWorkflow(**object_utils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The LangChain Expression Language Agent Workflow object provided is invalid. {str(e)}", 400)
        
        if workflow is None:
            raise LangChainException("The LangChain Expression Language Agent Workflow object provided is invalid.", 400)

        return workflow
