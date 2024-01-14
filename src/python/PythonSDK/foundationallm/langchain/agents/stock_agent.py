from io import StringIO
from operator import itemgetter
import pandas as pd
import os

from azure.identity import DefaultAzureCredential
from azure.core.credentials import AzureKeyCredential
from azure.storage.blob import BlobServiceClient
from azure.storage.blob import ContainerClient

from langchain.agents import AgentExecutor, ZeroShotAgent
from langchain.chains import LLMChain
from langchain.memory import ConversationBufferMemory
from langchain.callbacks import get_openai_callback
from langchain.prompts import PromptTemplate

from langchain.text_splitter import CharacterTextSplitter
from langchain.document_loaders import PyPDFLoader

#Move to new langchain...
#from langchain_community.document_loaders import UnstructuredXMLLoader
#from langchain_community.document_loaders import TextLoader
#rom langchain_community.vectorstores.azuresearch import AzureSearch
#from langchain_openai import AzureOpenAIEmbeddings
from langchain.embeddings.openai import OpenAIEmbeddings

from langchain.document_loaders import UnstructuredXMLLoader
from langchain.document_loaders import TextLoader
from langchain.vectorstores.azuresearch import AzureSearch

from langchain.document_loaders.csv_loader import CSVLoader
from langchain.vectorstores import Chroma
from langchain.chains import ConversationalRetrievalChain, RetrievalQAWithSourcesChain

from foundationallm.config import Configuration
from foundationallm.langchain.agents import AgentBase
from foundationallm.langchain.language_models import LanguageModelBase
from foundationallm.models.orchestration import CompletionRequest, CompletionResponse
from foundationallm.langchain.retrievers import SearchServiceFilterRetriever

class StockAgent(AgentBase):
    """
    Agent for analyzing the contents for stock analysis
    """

    def __init__(self, completion_request: CompletionRequest,
                 llm: LanguageModelBase, config: Configuration):
        """
        Initializes a Stock agent.

        Note: The Stock agent supports a single file.

        Parameters
        ----------
        completion_request : CompletionRequest
            The completion request object containing the user prompt to execute, message history,
            and agent and data source metadata.
        llm : LanguageModelBase
            The language model to use for executing the completion request.
        config : Configuration
            Application configuration class for retrieving configuration settings.
        """
        self.prompt_prefix = completion_request.agent.prompt_prefix
        self.prompt_suffix = completion_request.agent.prompt_suffix
        self.question = completion_request.user_prompt
        self.llm = llm.get_completion_model(completion_request.language_model)
        self.message_history = completion_request.message_history

        self.data_source = completion_request.data_sources[0]

        self.search_endpoint = config.get_value(self.data_source.search_endpoint)
        self.search_key = config.get_value(self.data_source.search_key)

        self.vector_store_address = config.get_value(self.data_source.search_endpoint)
        self.vector_store_password = config.get_value(self.data_source.search_key)

        azure_endpoint = config.get_value(self.data_source.open_ai_endpoint)
        azure_key = config.get_value(self.data_source.open_ai_key)
        model = config.get_value(self.data_source.embedding_model) #"embeddings"

        #self.embeddings: AzureOpenAIEmbeddings = AzureOpenAIEmbeddings(azure_endpoint=azure_endpoint, openai_api_key=azure_key, deployment=model, chunk_size=1)
        #self.embeddings = OpenAIEmbeddings(deployment=model,chunk_size=1,openai_api_key=azure_key, openai_endpoint=azure_endpoint,openai_api_type="azure")

        self.embeddings = OpenAIEmbeddings(
                deployment=model,
                #model="text-embedding-ada-002",
                openai_api_base=azure_endpoint,
                openai_api_key=azure_key,
                openai_api_type="azure",
            )

        # Load the CSV file
        company = self.data_source.company
        retriever_mode = self.data_source.retriever_mode
        self.load_mode = self.data_source.load_mode

        self.index_name = self.data_source.index_name
        temp_sources = self.data_source.sources
        self.filters = []

        for source in temp_sources:
            self.filters.append(f"search.ismatch('{source}', 'metadata', 'simple', 'all')")

        if ( retriever_mode == "azure" ):
            local_path = f"{company}-financials"
            self.retriever = self.get_azure_retiever(local_path, embedding_field_name="content_vector", text_field_name="content", top_n=self.data_source.top_n)

        if ( retriever_mode == "chroma" ):
            local_path = f"/temp/{company}"
            self.retriever = self.get_chroma_retiever(local_path)

        tools = []

        memory = ConversationBufferMemory(memory_key="chat_history", return_messages=True)
        # Add previous messages to the memory
        for i in range(0, len(self.message_history), 2):
            history_pair = itemgetter(i,i+1)(self.message_history)
            for message in history_pair:
                if message.sender.lower() == 'user':
                    user_input = message.text
                else:
                    ai_output = message.text
            memory.save_context({"input": user_input}, {"output": ai_output})

        prompt = PromptTemplate(
            template=self.prompt_prefix,
            input_variables=["context", "question"], #"summaries", "question"
        )

        self.llm_chain = ConversationalRetrievalChain.from_llm(
            llm=self.llm,
            retriever=self.retriever,
            return_source_documents=False,
            memory=memory,
            chain_type="stuff",
            combine_docs_chain_kwargs={"prompt": prompt},
            verbose=True
        )

        print('done with init')

    @property
    def prompt_template(self) -> str:
        """
        Property for viewing the agent's prompt template.

        Returns
        str
            Returns the prompt template for the agent.
        """
        return self.agent.agent.llm_chain.prompt.template

    def run(self, prompt: str) -> CompletionResponse:
        """
        Executes a query against the contents of a CSV file.

        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.

        Returns
        -------
        CompletionResponse
            Returns a CompletionResponse with the CSV file query completion response,
            the user_prompt, and token utilization and execution cost details.
        """

        with get_openai_callback() as cb:
            return CompletionResponse(
                completion = self.llm_chain.invoke(self.question, return_only_outputs=True)['answer'],
                user_prompt = self.question,
                completion_tokens = cb.completion_tokens,
                prompt_tokens = cb.prompt_tokens,
                total_tokens = cb.total_tokens,
                total_cost = cb.total_cost
            )



    def get_azure_retiever(self, path, top_n=5, embedding_field_name="Embedding", text_field_name="Text"):

        credential = AzureKeyCredential(self.vector_store_password)

        return SearchServiceFilterRetriever(
                    endpoint=self.vector_store_address,
                    index_name=self.index_name,
                    filters=self.filters,
                    top_n=top_n,
                    embedding_field_name=embedding_field_name,
                    text_field_name=text_field_name,
                    credential=credential,
                    embedding_model=self.embeddings
            )

    def get_chroma_retiever(self, path):

        prsstdb = Chroma(
                persist_directory=path,
                embedding_function=self.embeddings
            )

        return prsstdb.as_retriever()
