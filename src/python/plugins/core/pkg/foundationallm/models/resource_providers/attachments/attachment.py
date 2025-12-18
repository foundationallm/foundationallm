from typing import Optional
from foundationallm.models.resource_providers import ResourceBase

class Attachment(ResourceBase):
    """
    Encapsulates the attachment model from resource provider.
    """
    content_type: Optional[str] = None
    path: str
