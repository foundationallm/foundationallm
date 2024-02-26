"""
Class: Prompt
Description: Prompt model from resource provider.
"""
from typing import Optional
from pydantic import BaseModel

class Prompt(BaseModel):
    """
    Encapsulates the prompt model from resource provider.
    """
    name: str
    type: Optional[str] = None
    object_id: Optional[str] = None
    description: str
    prefix: str
    suffix: Optional[str] = "" 
