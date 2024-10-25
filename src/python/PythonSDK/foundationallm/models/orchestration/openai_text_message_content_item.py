from pydantic import Field
from typing import Optional, List
from .message_content_item_base import MessageContentItemBase
from .openai_file_path_message_content_item import OpenAIFilePathMessageContentItem
from .message_content_item_types import MessageContentItemTypes

class OpenAITextMessageContentItem(MessageContentItemBase):
    """An OpenAI text message content item."""
    
    annotations: Optional[List[OpenAIFilePathMessageContentItem]] = Field(default_factory=list, alias="annotations")
    value: Optional[str] = Field(None, alias="value")
    
    def __init__(self, **data):
        super().__init__(**data)
        self.type = MessageContentItemTypes.TEXT
