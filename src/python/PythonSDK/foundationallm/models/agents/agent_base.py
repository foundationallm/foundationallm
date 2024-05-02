"""
Base Agent fulfilling the orchestration request.
"""
from pydantic import BaseModel
from typing import Optional
from foundationallm.models.orchestration import OrchestrationSettings
from foundationallm.models.agents import AgentConversationHistorySettings, AgentGatekeeperSettings

class AgentBase(BaseModel):
    """ Agent Base model."""
    name: str
    description: str
    type: str
    object_id: Optional[str] = None
    prompt_object_id: Optional[str] = None
    sessions_enabled: Optional[bool] = False
    conversation_history: Optional[AgentConversationHistorySettings] = AgentConversationHistorySettings()
    gatekeeper: Optional[AgentGatekeeperSettings] = AgentGatekeeperSettings()
    orchestration_settings: Optional[OrchestrationSettings] = OrchestrationSettings()
