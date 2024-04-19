"""
Encapsulates the settings for the conversation history of an agent.
"""
from typing import Optional
from pydantic import BaseModel

class AgentConversationHistorySettings(BaseModel):
    """Agent Conversation History Settings."""
    enabled: Optional[bool] = False
    max_history: Optional[int] = 10
