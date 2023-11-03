from typing import List
from foundationallm.config import Configuration
from langchain.base_language import BaseLanguageModel
from langchain.callbacks import get_openai_callback
from foundationallm.langchain.agents.agent_base import AgentBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from langchain.document_loaders import AzureBlobStorageFileLoader, AzureBlobStorageContainerLoader
from langchain.indexes import VectorstoreIndexCreator
from langchain.indexes.vectorstore import VectorStoreIndexWrapper
from langchain.embeddings import OpenAIEmbeddings
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.langchain.message_history import build_message_history

vector_store = {}

class BlobStorageAgent(AgentBase):
    """
    Agent for reading, indexing, and querying blobs from a blob storage container.
    """
        
    def __init__(self, completion_request: CompletionRequest, llm: BaseLanguageModel, config: Configuration):
        """
        Initializes a blob storage in-memory query agent.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.       
        """        
        self.llm = llm.get_language_model()
        self.prompt_prefix = completion_request.agent.prompt_template
        self.connection_string = config.get_value(completion_request.data_source.configuration.connection_string_secret)        
        self.container_name = completion_request.data_source.configuration.container        
        self.file_names = completion_request.data_source.configuration.files
        self.message_history = completion_request.message_history
        self.__config = config        
        
    def __get_vector_index(self) -> VectorStoreIndexWrapper:
        """
        Creates a vector index from files in the indicated blob storage container and files list
        """

        if self.container_name in vector_store:            
            return vector_store[self.container_name]
        
        loaders = []    
        if "*" in self.file_names:
            # Load all files in the container
            loaders.append(AzureBlobStorageContainerLoader(conn_str=self.connection_string, container=self.container_name))
        else:
            # Load specific files
            for file_name in self.file_names:
                loaders.append(AzureBlobStorageFileLoader(conn_str=self.connection_string, container=self.container_name, blob_name=file_name))
        
        # Optional parameters for VectorStoreIndexCreator: 
        #       embeddings (defaults to langchain's own embeddings), 
        #       text_splitter(defaults to TextSplitter)
        #       vectorstore_cls (defaults to Chroma)

        embeddings = OpenAIEmbeddings(
            openai_api_base = self.__config.get_value("FoundationaLLM:AzureOpenAI:API:Endpoint"),
            openai_api_version = self.__config.get_value("FoundationaLLM:AzureOpenAI:API:Version"),
            openai_api_key = self.__config.get_value("FoundationaLLM:AzureOpenAI:API:Key"),
            openai_api_type = "azure",
            deployment = "embeddings",
            chunk_size=1
        )
    
        index = VectorstoreIndexCreator(embedding=embeddings).from_loaders(loaders)
        vector_store[self.container_name] = index
        return index             
       
    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a completion request by querying the vector index with the user prompt.

        Parameters
        ----------
        prompt : str
            The prompt for which a summary completion is begin generated.
        
        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the generated summary, the user_prompt,
            and token utilization and execution cost details.
        """

        with get_openai_callback() as cb:
            index = self.__get_vector_index()
            query = self.prompt_prefix + build_message_history(self.message_history) + "Request: "+ prompt + "\n"            
            completion = index.query(query, self.llm)
            return CompletionResponse(
                completion = completion,
                user_prompt = prompt,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )
