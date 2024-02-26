"""
Classes:
    AzureAISearchSettings: Settings for an Azure AI Search indexing profile.
    AzureAISearchConfigurationReferences: Configuration references for an Azure AI Search indexing profile.
    AzureAISearchIndexingProfile: An Azure AI Search indexing profile.
Description:
    Settings specific to the Azure AI Search Indexes
"""
from foundationallm.models.resource_providers.vectorization import IndexingProfileBase
from .azure_ai_search_settings import AzureAISearchSettings
from .azure_ai_search_configuration_references import AzureAISearchConfigurationReferences

class AzureAISearchIndexingProfile(IndexingProfileBase):
    """
    An Azure AI Search indexing profile.
    """
    settings: AzureAISearchSettings
    configuration_references: AzureAISearchConfigurationReferences
