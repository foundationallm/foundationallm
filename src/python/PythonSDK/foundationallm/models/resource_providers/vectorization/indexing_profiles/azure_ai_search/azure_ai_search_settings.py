"""
Class: AzureAISearchSettings
Description: Settings for an Azure AI Search indexing profile.
"""
from typing import Optional
from foundationallm.models.resource_providers.vectorization import SettingsBase

class AzureAISearchSettings(SettingsBase):
    """
    Settings for an Azure AI Search indexing profile.
    """
    index_name: str
    top_n: Optional[str] = "3" # all settings are string
    filters: Optional[str] = ""
    embedding_field_name: Optional[str] = "Embedding"
    text_field_name: Optional[str] = "Text"
    metadata_field_name: Optional[str] = "AdditionalMetadata"
    id_field_name: Optional[str] = "Id"
