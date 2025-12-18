from pydantic import Field
from typing import Optional
from .message_content_item_base import MessageContentItemBase
from .message_content_item_types import MessageContentItemTypes

class OpenAIFilePathMessageContentItem(MessageContentItemBase):
    """File content item used to generate a message content item."""    
    
    start_index: Optional[int] = Field(None, alias="start_index")
    end_index: Optional[int] = Field(None, alias="end_index")
    file_id: Optional[str] = Field(None, alias="file_id")
    
    def __init__(self, **data):
        super().__init__(**data)
        self.type = MessageContentItemTypes.FILE_PATH
