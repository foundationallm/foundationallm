from typing import Optional
from pydantic import BaseModel

class MetadataBase(BaseModel):
    """Metadata model base class."""
    name: str
    type: Optional[str] = None
    description: str
