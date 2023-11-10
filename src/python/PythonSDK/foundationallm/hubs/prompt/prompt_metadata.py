from pydantic import BaseModel
from typing import Optional

class PromptMetadata(BaseModel):
    """Class representing the metadata for a prompt."""

    name: str    
    prompt_prefix: Optional[str] = None
    prompt_suffix: Optional[str] = None