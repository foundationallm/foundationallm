"""
Class: AzureAISearchServiceRetriever
Description: LangChain retriever for Azure AI Search.
"""
import json
from typing import List, Optional, Union, Tuple
from langchain_openai import OpenAIEmbeddings
from langchain_core.callbacks import (
    AsyncCallbackManagerForRetrieverRun,
    CallbackManagerForRetrieverRun,
)
from langchain_core.documents import Document
from langchain_core.retrievers import BaseRetriever
from azure.search.documents import SearchClient
from azure.search.documents.models import VectorizedQuery
from azure.core.credentials import AzureKeyCredential
from azure.identity import DefaultAzureCredential
from foundationallm.models.orchestration import Citation
from .citation_retrieval_base import CitationRetrievalBase

class AzureAISearchServiceRetriever(BaseRetriever, CitationRetrievalBase):
    """
    LangChain retriever for Azure AI Search.
    Properties:
        endpoint: str -> Azure AI Search endpoint
        index_name: str -> Azure AI Search index name
        top_n : int -> number of results to return from vector search
        embedding_field_name: str -> name of the field containing the embedding vector
        text_field_name: str -> name of the field containing the raw text
        id_field_name: str -> name of the field containing the document unique identifier in the index
        metadata_field_name -> str -> name of the field containing the JSON metadata as a string
        filters: str -> Azure AI Search filter expression
        credential: AzureKeyCredential -> Azure AI Search credential
        embedding_model: OpenAIEmbeddings -> OpenAIEmbeddings model

    Searches embedding and text fields in the index for the top_n most relevant documents.

    Default FFLM document structure (overridable by setting the embedding and text field names):
        {
            "Id": "<GUID>",
            "Embedding": [0.1, 0.2, 0.3, ...], # embedding vector of the Text
            "Text": "text of the chunk",
            "Description": "General description about the source of the text",            
            "AdditionalMetadata": "JSON string of metadata"
            "ExternalSourceName": "name and location the text came from, url, blob storage url"
            "IsReference": "true/false if the document is a reference document"
        }
    """
    endpoint: str
    index_name: str
    top_n : int
    embedding_field_name: Optional[str] = "Embedding"
    text_field_name: Optional[str] = "Text"
    id_field_name: Optional[str] = "Id"
    metadata_field_name: Optional[str] = "AdditionalMetadata"
    filters: Optional[str] = None
    credential: Union[AzureKeyCredential, DefaultAzureCredential] = None
    embedding_model: OpenAIEmbeddings
    search_results: Optional[Tuple[str, Document]] = [] # Tuple of document id and document

    def __get_embeddings(self, text: str) -> List[float]:
        """
        Returns embeddings vector for a given text.
        """
        embedding = self.embedding_model.embed_query(text)        
        return embedding

    def _get_relevant_documents(
        self, query: str, *, run_manager: CallbackManagerForRetrieverRun
    ) -> List[Document]:
        """
        Performs a synchronous hybrid search on Azure AI Search index        
        """        
        search_client = SearchClient(self.endpoint, self.index_name, self.credential)        
        vector_query = VectorizedQuery(vector=self.__get_embeddings(query),
                                        k_nearest_neighbors=3,
                                        fields=self.embedding_field_name)

        results = search_client.search(
            search_text=query,
            filter=self.filters,
            vector_queries=[vector_query],
            top=self.top_n,
            select=[self.id_field_name, self.text_field_name, self.metadata_field_name]
        )
        self.search_results.clear()
        for result in results:
            metadata = json.loads(result[self.metadata_field_name]) if self.metadata_field_name in result else {}
            document = Document(
                    page_content=result[self.text_field_name],
                    metadata=metadata                                        
            )
            self.search_results.append((result[self.id_field_name], document))
        return [doc for _, doc in self.search_results]

    async def _aget_relevant_documents(
        self, query: str, *, run_manager: AsyncCallbackManagerForRetrieverRun
    ) -> List[Document]:
        """
        Performs an asynchronous hybrid search on Azure AI Search index
        NOTE: This functionality is not currently supported in the underlying Azure SDK.
        """
        raise Exception(f"Asynchronous search not supported.")

    def get_document_citations(self) -> List[Citation]:
        """
        Gets sources from the documents retrieved from the retriever.
  
        Returns:
            List of citations from the retrieved documents.
        """
        citations = []
        added_ids = set()  # Avoid duplicates
        for result_id, result in self.search_results:  # Unpack the tuple
            metadata = result.metadata
            if metadata is not None and 'multipart_id' in metadata and metadata['multipart_id']:
                if result_id not in added_ids:          
                    title = (metadata['multipart_id'][-1]).split('/')[-1]
                    filepath = '/'.join(metadata['multipart_id'])
                    citations.append(Citation(id=result_id, title=title, filepath=filepath))
                    added_ids.add(result_id)
        return citations
