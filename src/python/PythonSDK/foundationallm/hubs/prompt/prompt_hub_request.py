from typing import Optional
from pydantic import BaseModel

class PromptHubRequest(BaseModel):
    """     
    PromptHubRequest contains the information needed to retrieve prompts from the PromptHub.
    
    agent_name: the name of the agent to retrieve prompts for,
                send an empty string or None to retrieve all prompts.
    session_id: the session ID value; can be used for caching.
    """
    agent_name: Optional[str] = None
    session_id:Optional[str] = None
    prompt_name: Optional[str] = 'default'
