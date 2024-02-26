"""
Class: AzureAISearchConfigurationReferences
Description:
    Configuration references for an Azure AI Search indexing profile.
"""
from typing import Optional
from foundationallm.models.resource_providers.vectorization import ConfigurationReferencesBase

class AzureAISearchConfigurationReferences(ConfigurationReferencesBase):
    """
    Configuration references for an Azure AI Search indexing profile.
    """
    api_key: Optional[str] = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:APIKey"  
    query_api_key: Optional[str] = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:QueryAPIKey"  
    authentication_type: Optional[str] = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType"  
    endpoint: Optional[str] = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
