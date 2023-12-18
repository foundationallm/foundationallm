"""
X-AGENT-HINT header model.
"""
from typing import Optional
from pydantic import BaseModel

class AgentHint(BaseModel):
    """
    AgentHint is the model for the X-AGENT-HINT header.
    Properties:
        name: the name of the agent to resolve.
        private(optional): boolean flag indicating whether
                the agent is private or global.
    """
    name: str
    private: Optional[bool] = False
