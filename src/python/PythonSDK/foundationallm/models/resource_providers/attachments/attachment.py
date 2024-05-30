from typing import Optional
from pydantic import BaseModel

class Attachment(BaseModel):
    """
    Encapsulates the attachment model from resource provider.
    """
    name: str
    type: Optional[str] = None
    content_type: Optional[str] = None
    object_id: Optional[str] = None
    description: Optional[str] = None
    path: str
