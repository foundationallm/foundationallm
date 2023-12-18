from typing import Optional
from pydantic import BaseModel

class AgentHubRequest(BaseModel):
    """     
    AgentHubRequest contains the information needed to retrieve AgentMetadata from the AgentHub.
    
    user_prompt: the prompt the user entered into the system.
    session_id: the session ID value; can be used for caching.
        
    """
    user_prompt:str
    session_id:Optional[str] = None
