from pydantic import BaseModel
from typing import Optional

class ResourceName(BaseModel):
    """
    A named resource with a type.
    """
    name: str
    type: Optional[str] = None
