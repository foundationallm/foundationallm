from typing import Optional
from pydantic import BaseModel

class PromptHubRequest(BaseModel):
    """     
    PromptHubRequest contains the information needed to retrieve prompts from the PromptHub.
    
    prompt_container: The prompt container from which prompt values will be retrieved.
                Send an empty string or None to retrieve all prompts.
    session_id: the session ID value; can be used for caching.
    """
    prompt_container: Optional[str] = None
    session_id:Optional[str] = None
    prompt_name: Optional[str] = 'default'
