from pydantic import BaseModel, Field
from typing import Optional

class FileHistoryItem(BaseModel):
    """
    Represents a file attachment uploaded into the context of a conversation.
    """
    order: int = Field(..., description="The order the file was uploaded in the current conversation.")
    current_message_attachment: Optional[bool] = Field(False, description="Indicates if the file is the attached to the current message.")
    original_file_name: str = Field(..., description="The original file name of the attachment.")
    object_id: str = Field(..., description="The ObjectID of the file attachment resource.")
    file_path: str = Field(..., description="The file path of the attachment in storage.")
    content_type: Optional[str] = Field(None, description="The content type of the attachment.")
    secondary_provider: Optional[str] = Field(None, description="The secondary provider of the attachment.")
    secondary_provider_object_id: Optional[str] = Field(None, description="The identifier of the attachment in the secondary provider.")
