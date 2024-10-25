from typing import Optional
from pydantic import BaseModel, Field

class AgentOrchestrationSettings(BaseModel):
    """
    Represents settings for the orchestration.
    """
    orchestrator: Optional[str] = Field(default='LangChain', description="The agent's LLM orchestrator type.")
    agent_parameters: Optional[dict] = Field(default=None, description="A dictionary containing override values for the agent parameters.")
