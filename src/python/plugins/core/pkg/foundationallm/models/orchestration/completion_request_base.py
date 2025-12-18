from typing import List, Optional
from pydantic import BaseModel, Field
from foundationallm.models.attachments import AttachmentProperties
from foundationallm.models.messages import MessageHistoryItem
from .file_history_item import FileHistoryItem

class CompletionRequestBase(BaseModel):
    """
    Base class for completion requests.
    """
    operation_id: str = Field(description="The operation ID for the completion request.")
    session_id: Optional[str] = Field(None, description="The session ID for the completion request.")
    user_prompt: str = Field(description="The user prompt for the completion request.")
    user_prompt_rewrite: Optional[str] = Field(None, description="The user prompt rewrite for the completion request.")
    message_history: Optional[List[MessageHistoryItem]] = Field(list, description="The message history for the completion.")
    file_history: Optional[List[FileHistoryItem]] = Field(list, description="The file history for the completion request.")
    attachments: Optional[List[AttachmentProperties]] = Field(list, description="The attachments collection for the completion request.")
