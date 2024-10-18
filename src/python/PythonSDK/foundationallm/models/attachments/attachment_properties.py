from pydantic import BaseModel, Field
from typing import Optional

class AttachmentProperties(BaseModel):
    """
    Properties of an attachment.
    """
    original_file_name: Optional[str] = Field(None, description="The original file name of the attachment.")
    content_type: Optional[str] = Field(None, description="The content/mime type of the attachment.")
    provider: Optional[str] = Field(None, description="The provider of the attachment.")
    provider_file_name: Optional[str] = Field(None, description="The provider's file name/URL of the attachment.")
    provider_storage_account_name: Optional[str] = Field(None, description="The storage account hosting the attachment.")
