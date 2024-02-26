"""
Class Name: AgentBase

Description: Encapsulates the common metadata for the agent
fulfilling the orchestration request.
"""
from typing import Optional
from foundationallm.models.language_models import LanguageModel
from .metadata_base import MetadataBase
from .conversation_history import ConversationHistory
from .gatekeeper import Gatekeeper

class AgentBase(MetadataBase):
    """ Agent Base metadata model."""
    prompt_object_id: Optional[str] = None
    language_model: Optional[LanguageModel] = LanguageModel()
    sessions_enabled: Optional[bool] = False
    conversation_history: Optional[ConversationHistory] = ConversationHistory()
    gatekeeper: Optional[Gatekeeper] = Gatekeeper()
    orchestrator: Optional[str] = "LangChain" # used only for agent hub
    
