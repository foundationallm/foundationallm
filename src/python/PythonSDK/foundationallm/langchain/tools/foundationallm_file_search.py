from typing import Optional, Type
from langchain_core.callbacks import CallbackManagerForToolRun, AsyncCallbackManagerForToolRun
from langchain_core.runnables import RunnableLambda, RunnablePassthrough
from langchain_core.tools import ToolException

from foundationallm.config import Configuration
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.retrievers.retriever_factory import RetrieverFactory
from foundationallm.models.agents import AgentTool, KnowledgeManagementIndexConfiguration
from foundationallm.models.orchestration import CompletionRequestObjectKeys
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.models.resource_providers.vectorization import (
    AzureOpenAIEmbeddingProfile,
    EmbeddingProfileSettingsKeys,
    AzureAISearchIndexingProfile)
from foundationallm.services.gateway_text_embedding import GatewayTextEmbeddingService
from .fllm_tool_base import FLLMToolBase

class FoundationaLLMFileSearchTool(FLLMToolBase):
    """
    FoundationaLLM file search tool.    
    """
    #args_schema: Type[BaseModel] = FoundationaLLMFileSearchToolInput
        
    def __init__(self, tool_config: AgentTool, objects: dict, config: Configuration):
        """ Initializes the FoundationaLLMFileSearchTool class with the tool configuration,
            exploded objects collection, and platform configuration. """
        super().__init__(tool_config, objects, config)
        self.tool_config = tool_config
        self.objects = objects
        self.retriever = self._get_document_retriever()
        self.full_prompt = ""
        self.client = self._get_client()

    def _run(self,                 
            prompt: str,            
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None
            ) -> str:
        raise ToolException("This tool does not support synchronous execution. Please use the async version of the tool.")
    
    async def _arun(self,                 
            prompt: str,           
            run_manager: Optional[AsyncCallbackManagerForToolRun] = None
            ) -> str:

        rag_prompt += "\n\nContext:\n{context}\n\nQuestion:{question}"
        
        rag_chain = (
            {"context": self.retriever | self.retriever.format_docs, "question": RunnablePassthrough() }
            | rag_prompt
            | RunnableLambda(self.record_full_prompt)
            | self.client
            )
        completion = await rag_chain.ainvoke(prompt)
        # TO DO: return citations (content artifact), token usage, and full prompt.
        # https://python.langchain.com/docs/how_to/tool_artifacts
        return completion.content

    def _get_document_retriever(self):
        """
        Get the vector document retriever
        """
        retriever = None

        # Well-known key for embedding profile object id (embedding_profile_object_id)
        text_embedding_profile = AzureOpenAIEmbeddingProfile.from_object(
            self.objects[self.tool_config.properties.get("text_embedding_profile_object_id")]
        )
        
        # Well-known key for the associated indexing profiles (indexing_profile_object_ids)
        indexing_profile_object_ids = self.tool_config.properties.get("indexing_profile_object_ids")
            
        # text_embedding_profile has the embedding model name in Settings.
        text_embedding_model_name = text_embedding_profile.settings.get(EmbeddingProfileSettingsKeys.MODEL_NAME)
            
        # Only supporting GatewayTextEmbedding
        # Objects dictionary has the gateway API endpoint configuration by default.
        gateway_endpoint_configuration = APIEndpointConfiguration.from_object(
            self.objects[CompletionRequestObjectKeys.GATEWAY_API_ENDPOINT_CONFIGURATION]
        )
            
        gateway_embedding_service = GatewayTextEmbeddingService(
            instance_id= self.instance_id,
            user_identity=self.user_identity,
            gateway_api_endpoint_configuration=gateway_endpoint_configuration,
            model_name = text_embedding_model_name,
            config=self.config
            )
            
        # array of objects containing the indexing profile(s) and associated endpoint configuration
        index_configurations = []                        
        for profile_id in indexing_profile_object_ids:
            indexing_profile = AzureAISearchIndexingProfile.from_object(
                    self.objects[profile_id]
                )
            # indexing profile has indexing_api_endpoint_configuration_object_id in Settings.                    
            indexing_api_endpoint_configuration = APIEndpointConfiguration.from_object(
                self.objects[indexing_profile.settings.api_endpoint_configuration_object_id]
            )      
            index_configurations.append(
                KnowledgeManagementIndexConfiguration(
                    indexing_profile = indexing_profile,
                    api_endpoint_configuration = indexing_api_endpoint_configuration
                )
            )
        retriever_factory = RetrieverFactory(
                        index_configurations=index_configurations,
                        gateway_text_embedding_service=gateway_embedding_service,
                        config=self.config)
        retriever = retriever_factory.get_retriever()
        return retriever

    def _record_full_prompt(self, prompt: str) -> str:
        """
        Records the full prompt for the completion request.

        Parameters
        ----------
        prompt : str
            The prompt that is populated with context.
        
        Returns
        -------
        str
            Returns the full prompt.
        """
        self.full_prompt = prompt
        return prompt

    def _get_client(self):
        """ Creates a client for the FoundationaLLM file search tool. """
        language_model_factory = LanguageModelFactory(self.objects, self.config)
        self.client = language_model_factory._get_language_model(self.tool_config.ai_model_object_ids["main_model"])
        

