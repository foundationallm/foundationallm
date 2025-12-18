from pydantic import BaseModel, Field
from typing import Optional
from .message_content_item_types import MessageContentItemTypes
from foundationallm.models.constants import AgentCapabilityCategories

class MessageContentItemBase(BaseModel):
    """Base message content item model."""

    type: Optional[str] = Field(None, description="The type of message content item.")
    text: Optional[str] = Field(None, alias="text")
    agent_capability_category: Optional[AgentCapabilityCategories] = Field(None, description="The category of capability assigned to the agent.")
    
    class Config:
        use_enum_values = True
        populate_by_name = True
        extra = "forbid"
