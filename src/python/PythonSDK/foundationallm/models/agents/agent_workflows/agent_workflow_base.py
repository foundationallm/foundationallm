from pydantic import BaseModel, Field
from typing import Any, Self, Optional, Dict
from .agent_workflow_ai_model import AgentWorkflowAIModel
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException

class AgentWorkflowBase(BaseModel):
    """
    The base class used for an agent workflow.
    """
    type: Optional[str] = Field(None, alias="type")
    workflow_object_id: str = Field(..., alias="workflow_object_id")
    workflow_name: str = Field(..., alias="workflow_name")
    agent_workflow_ai_models: Dict[str, AgentWorkflowAIModel] = Field(default_factory=dict, alias="agent_workflow_ai_models")
    prompt_object_ids: Dict[str, str] = Field(default_factory=dict, alias="prompt_object_ids")

    @staticmethod
    def from_object(obj: Any) -> Self:
        agent_workflow_base: AgentWorkflowBase = None
        try:
            agent_workflow_base = AgentWorkflowBase(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The Agent Workflow base model object provided is invalid. {str(e)}", 400)
        
        if agent_workflow_base is None:
            raise LangChainException("The Agent Workflow base model object provided is invalid.", 400)

        return agent_workflow_base
