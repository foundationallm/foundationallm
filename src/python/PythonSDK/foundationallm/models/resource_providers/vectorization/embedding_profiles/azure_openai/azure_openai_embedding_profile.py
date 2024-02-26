"""
Classes:   
    AzureAISearchIndexingProfile: An Azure AI Search indexing profile.
Description:
    Settings specific to the Azure AI Search Indexes
"""
from typing import Optional
from foundationallm.models.resource_providers.vectorization import SettingsBase, EmbeddingProfileBase
from .azure_openai_configuration_references import AzureOpenAIConfigurationReferences

class AzureOpenAIEmbeddingProfile(EmbeddingProfileBase):
    """
    An Azure AI Search indexing profile.
    """
    settings: Optional[SettingsBase] = None
    configuration_references: AzureOpenAIConfigurationReferences
