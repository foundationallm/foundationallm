"""
Class: AzureAISearchConfigurationReferences
Description:
    Configuration references for an Azure AI Search indexing profile.
"""
from pydantic import BaseModel
from typing import Optional

class AzureAISearchConfigurationReferences(BaseModel):
    """
    Configuration references for an Azure AI Search indexing profile.
    """
    authentication_type: Optional[str] = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:AuthenticationType"  
    endpoint: Optional[str] = "FoundationaLLM:Vectorization:AzureAISearchIndexingService:Endpoint"
