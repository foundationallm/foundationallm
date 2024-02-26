"""
Class: AzureOpenAIConfigurationReferences
Description:
    Configuration references for an Azure OpenAI text embedding service.
"""
from typing import Optional
from foundationallm.models.resource_providers.vectorization import ConfigurationReferencesBase

class AzureOpenAIConfigurationReferences(ConfigurationReferencesBase):
    """
    Configuration references for an Azure OpenAI text embedding service.
    """
    api_key: Optional[str] = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIKey"
    api_version: Optional[str] = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:APIVersion"
    authentication_type: Optional[str] = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:AuthenticationType"
    deployment_name: Optional[str] = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:DeploymentName"
    endpoint: Optional[str] = "FoundationaLLM:Vectorization:SemanticKernelTextEmbeddingService:Endpoint"
