from typing import Optional
from pydantic import BaseModel

class Citation(BaseModel):
    """
    Source reference information for a completion
    """
    id: str # id in the index
    title: Optional[str] = None # file name
    filepath: Optional[str] = None
