"""
Class: AzureAISearchSettings
Description: Settings for an Azure AI Search indexing profile.
"""
from pydantic import BaseModel
from typing import Optional

class AzureAISearchSettings(BaseModel):
    """
    Settings for an Azure AI Search indexing profile.
    """
    index_name: str
    top_n: Optional[str] = "3" # all settings are string
    filters: Optional[str] = ""
    api_endpoint_configuration_object_id: Optional[str] = ""
    embedding_field_name: Optional[str] = "Embedding"
    text_field_name: Optional[str] = "Text"
    metadata_field_name: Optional[str] = "AdditionalMetadata"
    id_field_name: Optional[str] = "Id"
