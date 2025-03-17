from pydantic import BaseModel, Field
from typing import Optional

class FileHistoryItem(BaseModel):
    """
    Represents a file attachment uploaded into the context of a conversation.
    """
    order: int = Field(..., description="The order the file was uploaded in the current conversation.")
    original_file_name: str = Field(..., description="The original file name of the attachment.")
    object_id: str = Field(..., description="The ObjectID of the file attachment resource.")
    file_path: str = Field(..., description="The file path of the attachment in storage.")
    content_type: Optional[str] = Field(None, description="The content type of the attachment.")
