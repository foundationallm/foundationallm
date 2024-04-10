from typing import Optional
from azure.core.credentials import AzureKeyCredential
from azure.identity import DefaultAzureCredential
from langchain_core.retrievers import BaseRetriever
from foundationallm.config import Configuration
from foundationallm.langchain.language_models.openai import OpenAIModel
from foundationallm.models.orchestration import OrchestrationSettings
from foundationallm.models.language_models import EmbeddingModel, LanguageModelType, LanguageModelProvider
from foundationallm.resources import ResourceProvider
from .agent_parameter_retriever_keys import FILTERS, TOP_N
from .azure_ai_search_service_retriever import AzureAISearchServiceRetriever

class RetrieverFactory:
    """
    Factory class for determine which retriever to use.
    """
    def __init__(
                self,
                indexing_profile_object_id: str,
                text_embedding_profile_object_id:str,
                config: Configuration,
                resource_provider: ResourceProvider,
                settings: Optional[OrchestrationSettings] = None
                ):
        self.config = config
        self.resource_provider = resource_provider
        self.indexing_profile = resource_provider.get_resource(indexing_profile_object_id)
        self.text_embedding_profile = resource_provider.get_resource(text_embedding_profile_object_id)
        self.orchestration_settings = settings       

    def get_retriever(self) -> BaseRetriever:
        """
        Retrieves the retriever to use for completion requests.
        
        Returns
        -------
        BaseRetriever
            Returns the concrete initialization of a vectorstore retriever.
        """

        # use embedding profile to build the embedding model (currently only supporting Azure OpenAI)         
        #embedding_model_type = self.text_embedding_profile["text_embedding"]
        #embedding_model = None
        #match embedding_model_type:            
        #    case "SemanticKernelTextEmbedding": # same as Azure Open AI Embedding        
        e_model = EmbeddingModel(
            type = LanguageModelType.OPENAI,
            provider = LanguageModelProvider.MICROSOFT,
            # the OpenAI model uses config to retrieve the app config values - pass in the keys
            deployment = self.text_embedding_profile.configuration_references.deployment_name,
            api_endpoint = self.text_embedding_profile.configuration_references.endpoint,
            api_key = self.text_embedding_profile.configuration_references.api_key,
            api_version = self.text_embedding_profile.configuration_references.api_version
        )
        oai_model = OpenAIModel(config = self.config)
        embedding_model = oai_model.get_embedding_model(e_model)
                  
        # use indexing profile to build the retriever (current only supporting Azure AI Search)
        #vector_store_type = self.indexing_profile["indexer"]        
        #match vector_store_type:
        #    case "AzureAISearchIndexer":
        
        credential_type = self.config.get_value(self.indexing_profile.configuration_references.authentication_type)
        credential = None
        if credential_type == "AzureIdentity":            
            credential = DefaultAzureCredential()
        # NOTE: Support for all other authentication types has been removed.

        # defaults for agent parameters
        top_n = self.indexing_profile.settings.top_n
        filters = self.indexing_profile.settings.filters
        # check for settings override       
        if self.orchestration_settings is not None:
            if self.orchestration_settings.agent_parameters is not None:
                if TOP_N in self.orchestration_settings.agent_parameters:
                    top_n = self.orchestration_settings.agent_parameters[TOP_N]                    
                if FILTERS in self.orchestration_settings.agent_parameters:
                    filters = self.orchestration_settings.agent_parameters[FILTERS]                    

        retriever = AzureAISearchServiceRetriever( 
            endpoint = self.config.get_value(self.indexing_profile.configuration_references.endpoint),
            index_name = self.indexing_profile.settings.index_name,
            top_n = top_n,
            embedding_field_name = self.indexing_profile.settings.embedding_field_name,
            text_field_name = self.indexing_profile.settings.text_field_name,
            id_field_name = self.indexing_profile.settings.id_field_name,
            metadata_field_name = self.indexing_profile.settings.metadata_field_name,
            filters = filters,
            credential = credential,            
            embedding_model = embedding_model
        )
        return retriever
