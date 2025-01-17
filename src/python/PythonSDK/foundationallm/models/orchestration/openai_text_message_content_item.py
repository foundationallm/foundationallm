from pydantic import Field
from typing import Optional, List, Literal
from .message_content_item_base import MessageContentItemBase
from .openai_file_path_message_content_item import OpenAIFilePathMessageContentItem
from .message_content_item_types import MessageContentItemTypes

class OpenAITextMessageContentItem(MessageContentItemBase):
    """An OpenAI text message content item."""

    type: Literal[MessageContentItemTypes.TEXT] = MessageContentItemTypes.TEXT
    annotations: Optional[List[OpenAIFilePathMessageContentItem]] = Field(default_factory=list, alias="annotations")
    value: Optional[str] = Field(None, alias="value")
