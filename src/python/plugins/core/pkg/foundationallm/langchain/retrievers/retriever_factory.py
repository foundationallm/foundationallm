from typing import List
from azure.identity import DefaultAzureCredential
from langchain_core.retrievers import BaseRetriever
from foundationallm.config import Configuration
from foundationallm.services.gateway_text_embedding import GatewayTextEmbeddingService
from .azure_ai_search_service_retriever import AzureAISearchServiceRetriever
from foundationallm.models.agents import KnowledgeManagementIndexConfiguration

class RetrieverFactory:
    """
    Factory class for determine which retriever to use.
    """
    def __init__(
        self,
        index_configurations: List[KnowledgeManagementIndexConfiguration],
        gateway_text_embedding_service: GatewayTextEmbeddingService,
        config: Configuration):

        self.config = config
        self.index_configurations = index_configurations
        self.gateway_text_embedding_service = gateway_text_embedding_service
        
    def get_retriever(self) -> BaseRetriever:
        """
        Returns the retriever to use for completion requests.

        Returns
        -------
        BaseRetriever
            Returns the concrete initialization of a vectorstore retriever.
        """               
        credential = DefaultAzureCredential(exclude_environment_credential=True)                                   
  
        """
        # use indexing profile to build the retriever (current only supporting Azure AI Search)
        top_n = self.indexing_profile.settings.top_n
        filters = self.indexing_profile.settings.filters
        """

        retriever = AzureAISearchServiceRetriever(
            config=self.config,
            index_configurations=self.index_configurations,
            gateway_text_embedding_service=self.gateway_text_embedding_service
        )

        return retriever
