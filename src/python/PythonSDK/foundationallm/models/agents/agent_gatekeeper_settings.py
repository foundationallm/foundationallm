"""
Agent Gatekeeper settings.
"""
from typing import Optional, List
from pydantic import BaseModel

class AgentGatekeeperSettings(BaseModel):
    """Agent Gatekeeper settings"""
    use_system_setting: bool = True
    options:Optional[List[str]] = None
