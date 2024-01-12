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
from foundationallm.storage import BlobStorageManager
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

        self.storage_connection_string = config.get_value(completion_request.data_source.configuration.connection_string_secret)
        self.container_name = completion_request.data_source.configuration.container

        self.storage_manager = BlobStorageManager(
            self.storage_connection_string, self.container_name
        )

        self.blob_service_client = self.storage_manager.blob_service_client

        self.search_endpoint = config.get_value(completion_request.data_source.configuration.search_endpoint)
        self.search_key = config.get_value(completion_request.data_source.configuration.search_key)

        self.vector_store_address = config.get_value(completion_request.data_source.configuration.search_endpoint)
        self.vector_store_password = config.get_value(completion_request.data_source.configuration.search_key)

        azure_endpoint = config.get_value(completion_request.data_source.configuration.open_ai_endpoint)
        azure_key = config.get_value(completion_request.data_source.configuration.open_ai_key)
        model = config.get_value(completion_request.data_source.configuration.embedding_model) #"embeddings"

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
        company = completion_request.data_source.configuration.company
        retriever_mode = completion_request.data_source.configuration.retriever_mode
        load_mode = completion_request.data_source.configuration.load_mode

        self.index_name = completion_request.data_source.configuration.index_name
        temp_sources = completion_request.data_source.configuration.sources
        self.filters = []

        for source in temp_sources:
            self.filters.append(f"search.ismatch('{source}', 'metadata', 'simple', 'all')")

        if ( retriever_mode == "azure" ):
            local_path = f"{company}-financials"
            retriever = self.get_azure_retiever(local_path, embedding_field_name="content_vector", text_field_name="content")

        if ( retriever == "chroma" ):
            local_path = f"c:/temp/{company}"
            #retriever = self.load_csvs(local_path, retriever_mode=retriever_mode, load_mode=load_mode)
            #retriever = self.load_10q(local_path, retriever_mode=retriever_mode, load_mode=load_mode)
            retriever = self.get_chroma_retiever(local_path)

            download = False

            if ( download == True ):
                #download the files from storage account...
                self.download(path=local_path)
                self.load_data(retriever_mode=retriever_mode, local_path=local_path, load_mode=load_mode)

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
            retriever=retriever,
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

    def who_is_question_for(self, question):

        template = """
        There are three executives in an earnings call taking questions from analysts.  You are to determine who the question is directed at.  You can pick from the CEO, Kevin Blair.  The CFO Andrew Gregory Jr. or the CCO Robert Derrick.  Only response with 'CEO', 'CFO' or 'CCO'.
        Question:
        {input}
        """

        llm_prompt = PromptTemplate(
            input_variables=["input"],
            template=template,
        )

        chain = LLMChain(llm=self.llm, prompt=llm_prompt)

        res = chain.invoke({"input" : question}, return_only_outputs=True) # prompt is human input from request body

        print(f'{res["text"]} : {question}')

    def chroma_vectorize(self, docs, path, chunk_size=1000, chunk_overlap=0):
        print(f"Sending docs to Chroma")
        text_splitter = CharacterTextSplitter(chunk_size=chunk_size, chunk_overlap=chunk_overlap)
        csv_texts = text_splitter.split_documents(docs)
        csv_db = Chroma.from_documents(csv_texts, self.embeddings, persist_directory=path)
        csv_retriever = csv_db.as_retriever()
        return csv_retriever

    def azure_search_vectorize(self, docs, path, chunk_size=1000, chunk_overlap=0):
        from azure.search.documents.indexes.models import (
            SearchIndex,
            SearchField,
            SearchFieldDataType,
            SimpleField,
            SearchableField,
            VectorSearch,
            HnswAlgorithmConfiguration
        )

        print(f"Sending docs to Azure Search")
        text_splitter = CharacterTextSplitter(chunk_size=chunk_size, chunk_overlap=chunk_overlap)
        docs = text_splitter.split_documents(docs)

        default_fields = [
            SimpleField(
                name="id",
                type=SearchFieldDataType.String,
                key=True,
                filterable=True,
            ),
            SearchableField(
                name="Text",
                type=SearchFieldDataType.String,
            ),
            SearchField(
                name="Embedding",
                type=SearchFieldDataType.Collection(SearchFieldDataType.Single),
                searchable=True,
                vector_search_dimensions=len(self.embeddings.embed_query("Text")),
                vector_search_configuration="default",
            ),
            SearchableField(
                name="AdditionalMetadata",
                type=SearchFieldDataType.String,
            ),
        ]

        vector_store: AzureSearch = AzureSearch(
            azure_search_endpoint=self.vector_store_address,
            azure_search_key=self.vector_store_password,
            index_name=path,
            embedding_function=self.embeddings.embed_query,
            embedding_size=1536,
            fields=default_fields,
            #api_version="2023-07-01-Preview"
            connection_verify=False
        )

        vector_store.add_documents(documents=docs, connection_verify=False)

        credential = AzureKeyCredential(self.vector_store_password)

        return SearchServiceFilterRetriever(
                    endpoint=self.vector_store_address,
                    indexes = self.sources,
                    index_name="cjg-vector-index",
                    top_n=5,
                    embedding_field_name="Embedding",
                    text_field_name="Text",
                    credential=credential,
                    embedding_model=self.embeddings
            )

    def get_azure_retiever(self, path="cjg-vector-index", top_n=5, embedding_field_name="Embedding", text_field_name="Text"):

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

    def get_chroma_retiever(self, path="cjg-vector-index"):

        prsstdb = Chroma(
                persist_directory=path,
                embedding_function=self.embeddings
            )

        return prsstdb.as_retriever()

    def load_data(self, retriever_mode = "azure", local_path="", load_mode="load"):

        csv_retriever = self.load_csvs(local_path, retriever_mode=retriever_mode, load_mode=load_mode)

        pdf_retriever = self.load_pdfs(local_path, retriever_mode=retriever_mode, load_mode=load_mode)

        docs_10k_retriever = self.load_10k(local_path, retriever_mode=retriever_mode, load_mode=load_mode)

        docs_10q_retriever = self.load_10q(local_path, retriever_mode=retriever_mode, load_mode=load_mode)

        docs_14a_retriever = self.load_14a(local_path, retriever_mode=retriever_mode, load_mode=load_mode)

        transcripts_retriever = self.load_transcripts(local_path, retriever_mode=retriever_mode, load_mode=load_mode)

    def load_pdfs(self, path, retriever_mode = "azure", load_mode="load"):
        return self.load_pdf_docs(path, "pdfs", retriever_mode=retriever_mode, retriever_path="/synovus/pdfs", chunk_size=1000, chunk_overlap=0)

    def load_competitors(self, path, retriever_mode = "azure", load_mode="load"):
        return self.load_xml_docs(path, "competitors", retriever_mode=retriever_mode, retriever_path="/synovus/competitors", chunk_size=1000, chunk_overlap=0)

    def load_10k(self, path, retriever_mode = "azure", load_mode="load"):
        return self.load_xml_docs(path, "10k", retriever_mode=retriever_mode, retriever_path="/synovus/10k", chunk_size=1000, chunk_overlap=0)

    def load_10q(self, path, retriever_mode = "azure", load_mode="load"):
        return self.load_xml_docs(path, "10q", retriever_mode=retriever_mode, retriever_path="/synovus/10q", chunk_size=1000, chunk_overlap=0)

    def load_14a(self, path, retriever_mode = "azure", load_mode="load"):
        return self.load_xml_docs(path, "14a", retriever_mode=retriever_mode, retriever_path="/synovus/14a", chunk_size=1000, chunk_overlap=0)

    def load_transcripts(self, path, retriever_mode = "azure", load_mode="load"):
        return self.load_text_docs(path, "transcripts", retriever_mode=retriever_mode, retriever_path="/synovus/transcripts", chunk_size=1000, chunk_overlap=0)

    def load_csvs(self, path, retriever_mode = "azure", load_mode="load"):
        return self.load_csv_docs(path, "csvs", retriever_mode=retriever_mode, retriever_path="/synovus/csvs", chunk_size=1000, chunk_overlap=0, load_mode=load_mode)

    def vectorize_docs(self, docs, retriever_mode = "azure", retriever_path = "", chunk_size=1000, chunk_overlap=0, load_mode="load"):

        retriever = None

        if ( load_mode == "index" ):

            print(f'Vectorizing docs to {retriever_mode}')

            if ( retriever_mode == "azure" ):
                retriever = self.azure_search_vectorize(docs, '')

            if ( retriever_mode == "chroma" ):
                retriever = self.chroma_vectorize(docs, retriever_path, chunk_size=chunk_size, chunk_overlap=chunk_overlap)

        print(f'Getting retriever {retriever_mode}')

        if ( retriever_mode == "azure" ):

            credential = AzureKeyCredential(self.vector_store_password)

            retriever = SearchServiceRetriever(
                    endpoint=self.vector_store_address,
                    index_name="cjg-vector-index",
                    top_n=5,
                    embedding_field_name="Embedding",
                    text_field_name="Text",
                    credential=credential,
                    embedding_model=self.embeddings
            )

        if ( retriever_mode == "chroma" ):
            prsstdb = Chroma(
                persist_directory=retriever_path,
                embedding_function=self.embeddings
            )

            retriever = prsstdb.as_retriever()

        return retriever

    def load_pdf_docs(self, path, local_path, retriever_mode = "azure", retriever_path = "", chunk_size=1000, chunk_overlap=0, load_mode="load"):

        print(f'Loading pdf docs {path}/{local_path}')

        docs = None
        files = os.listdir(f"{path}/{local_path}")

        for file in files:

            if ( file.endswith(".pdf") == False ):
                continue

            print(f'\tLoading {path}/{local_path}/{file}')

            loader = PyPDFLoader(
                f"{path}/{local_path}/{file}",
            )

            doc = loader.load()

            if ( docs == None ):
                docs = doc
            else:
                docs += doc

        return self.vectorize_docs(docs, retriever_path=retriever_path, retriever_mode=retriever_mode, load_mode=load_mode)

    def load_text_docs(self, path, local_path, retriever_mode = "azure", retriever_path = "", chunk_size=1000, chunk_overlap=0, load_mode="load"):

        print(f'Loading text docs {path}/{local_path}')

        docs = None
        files = os.listdir(f"{path}/{local_path}")

        for file in files:

            print(f'\tLoading {path}/{local_path}/{file}')

            loader = TextLoader(
                f"{path}/{local_path}/{file}",
            )

            doc = loader.load()

            if ( docs == None ):
                docs = doc
            else:
                docs += doc

        return self.vectorize_docs(docs, retriever_path=retriever_path, retriever_mode=retriever_mode, load_mode=load_mode)

    def load_xml_docs(self, path, local_path, retriever_mode = "azure", retriever_path = "", chunk_size=1000, chunk_overlap=0, load_mode="load"):

        docs = None

        if (load_mode == "index"):
            print(f'Loading xml docs {path}/{local_path}')

            files = os.listdir(f"{path}/{local_path}")

            for file in files:

                print(f'\tLoading {path}/{local_path}/{file}')

                loader = UnstructuredXMLLoader(
                    f"{path}/{local_path}/{file}",
                )

                doc = loader.load()

                if ( docs == None ):
                    docs = doc
                else:
                    docs += doc

        return self.vectorize_docs(docs, retriever_path=retriever_path, retriever_mode=retriever_mode, load_mode=load_mode)

    def load_csv_docs(self, path, local_path, retriever_mode = "azure", retriever_path = "", chunk_size=1000, chunk_overlap=0, load_mode="load"):

        print(f'Loading csvs docs {path}/{local_path}')

        docs = None
        files = os.listdir(f"{path}/{local_path}")

        for file in files:
            if ( file.endswith(".csv") == False ):
                continue

            print(f'\tLoading {path}/{local_path}/{file}')

            loader = CSVLoader(
                f"{path}/{local_path}/{file}",
            )

            doc = loader.load()

            if ( docs == None ):
                docs = doc
            else:
                docs += doc

        return self.vectorize_docs(docs, retriever_path=retriever_path, retriever_mode=retriever_mode, load_mode=load_mode)

    def download_live_transcript(self, container_name="transcripts"):

        text = self.download_blob_to_text(self.blob_service_client, container_name, "transcripts/2021-04-20-earnings-call.txt")

        loader = TextLoader(text).load()

    #download the files from storage account...
    def download(self, local_path="", remote_file_path=""):

        container_client = self.blob_service_client.get_container_client(self.storage_manager.container_name)

        blob_list = container_client.list_blobs(remote_file_path)

        for blob in blob_list:
            print(blob.name + '\n')

            if( blob.size != 0):
                self.download_blob_to_file(self.blob_service_client, self.storage_manager.container_name, blob.name, local_path)

    def download_blob_to_text(self, blob_service_client: BlobServiceClient, container_name, blob_name, local_path):
        blob_client = blob_service_client.get_blob_client(container=container_name, blob=blob_name)

        file_path = f'{local_path}/{blob_name}'
        dir_path = os.path.split(file_path)[0]
        os.makedirs(dir_path, exist_ok=True)

        with open(file=os.path.join(r'filepath', f'{local_path}/{blob_name}'), mode="wb") as sample_blob:
            download_stream = blob_client.download_blob()
            sample_blob.write(download_stream.readall())

    def download_blob_to_file(self, blob_service_client: BlobServiceClient, container_name, blob_name, local_path):

        blob_client = blob_service_client.get_blob_client(container=container_name, blob=blob_name)

        file_path = f'{local_path}/{blob_name}'
        dir_path = os.path.split(file_path)[0]
        os.makedirs(dir_path, exist_ok=True)

        with open(file=os.path.join(r'filepath', f'{local_path}/{blob_name}'), mode="wb") as sample_blob:
            download_stream = blob_client.download_blob()
            sample_blob.write(download_stream.readall())

    #index them...
