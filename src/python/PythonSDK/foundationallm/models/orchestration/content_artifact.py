from typing import Optional
from pydantic import BaseModel

class ContentArtifact(BaseModel):
    """
    Source reference information for a completion
    """
    id: str # id in the index
    title: Optional[str] = None # file name
    filepath: Optional[str] = None
    source: Optional[str] = None
    content: Optional[str] = None
    type: Optional[str] = None
    metadata: Optional[dict] = None
