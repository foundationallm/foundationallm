from pydantic import Field
from typing import Optional
from .message_content_item_base import MessageContentItemBase
from .message_content_item_types import MessageContentItemTypes

class OpenAIImageFileMessageContentItem(MessageContentItemBase):
    """An OpenAI image file message content item."""
    
    file_id: Optional[str] = Field(None, alias="file_id")
    file_url: Optional[str] = Field(None, alias="file_url")
    
    def __init__(self, **data):
        super().__init__(**data)
        self.type = MessageContentItemTypes.IMAGE_FILE
