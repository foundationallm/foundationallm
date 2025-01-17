from pydantic import Field
from typing import Optional, Literal
from .message_content_item_base import MessageContentItemBase
from .message_content_item_types import MessageContentItemTypes

class OpenAIImageFileMessageContentItem(MessageContentItemBase):
    """An OpenAI image file message content item."""

    type: Literal[MessageContentItemTypes.IMAGE_FILE] = MessageContentItemTypes.IMAGE_FILE
    file_id: Optional[str] = Field(None, alias="file_id")
    file_url: Optional[str] = Field(None, alias="file_url")
