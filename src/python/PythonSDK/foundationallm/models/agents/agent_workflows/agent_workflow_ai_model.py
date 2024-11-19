from pydantic import BaseModel, Field
from typing import Any, Self, Optional
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException

class AgentWorkflowAIModel(BaseModel):
    """
    The base class used for a workflow AIModel resource and associated overrides.
    """
    ai_model_object_id: str = Field(description="The object ID of the AI model object for the workflow.")    
    model_parameters: Optional[dict] = Field(default={}, description="A dictionary containing override values for model parameters.")

    @staticmethod
    def from_object(obj: Any) -> Self:

        agent_workflow_ai_model: AgentWorkflowAIModel = None

        try:
            agent_workflow_ai_model = AgentWorkflowAIModel(**ObjectUtils.translate_keys(obj))
        except Exception as e:
            raise LangChainException(f"The Agent Workflow AI model object provided is invalid. {str(e)}", 400)
        
        if agent_workflow_ai_model is None:
            raise LangChainException("The Agent Workflow AI model object provided is invalid.", 400)

        return agent_workflow_ai_model
