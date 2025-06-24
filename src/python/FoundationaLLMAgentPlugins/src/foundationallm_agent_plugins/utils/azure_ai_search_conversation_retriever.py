"""
Class: AzureAISearchServiceRetriever
Description: LangChain retriever for Azure AI Search.
"""
from typing import List, Optional, Any
from langchain_openai import OpenAIEmbeddings
from langchain_core.callbacks import (
    AsyncCallbackManagerForRetrieverRun,
    CallbackManagerForRetrieverRun,
)
from langchain_core.documents import Document
from langchain_core.retrievers import BaseRetriever
from azure.search.documents import SearchClient
from azure.search.documents.models import VectorizedQuery, VectorSimilarityThreshold
from azure.identity import DefaultAzureCredential
from foundationallm.models.orchestration import ContentArtifact
from foundationallm.models.vectors import VectorDocument
from foundationallm.services.gateway_text_embedding import GatewayTextEmbeddingService
from foundationallm.langchain.retrievers.content_artifact_retrieval_base import ContentArtifactRetrievalBase
from foundationallm.models.agents import VectorDatabaseConfiguration

class AzureAISearchConversationRetriever(BaseRetriever, ContentArtifactRetrievalBase):
    """
    LangChain retriever for Azure AI Search.
    Properties:
        config: Any -> Application configuration
        vector_database_configuration: VectorDatabaseConfiguration
            -> Vector database and associated API endpoint configuration
        gateway_text_embedding_service: GatewayTextEmbeddingService
            -> Service for retrieving text embeddings

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
    vector_store_name: str
    file_name: Optional[str] = None
    vector_database_configuration: VectorDatabaseConfiguration
    gateway_text_embedding_service: GatewayTextEmbeddingService
    query_type: Optional[str] = "simple"
    semantic_configuration_name: Optional[str] = None

    def __get_embeddings(self, text: str) -> List[float]:
        """
        Returns embeddings vector for a given text.
        """
        embedding_response = self.gateway_text_embedding_service.get_embedding(text)
        return embedding_response.embedding_vector

    def _get_relevant_documents(
        self,
        query: str,
        *,
        run_manager: CallbackManagerForRetrieverRun
    ) -> List[Document]:
        """
        Performs a synchronous hybrid search on Azure AI Search index
        """
        search_results: List[VectorDocument] = []

        # Search
        index_config = self.vector_database_configuration
        credential_type = index_config.vector_database_api_endpoint_configuration.authentication_type

        credential = None
        if credential_type == "AzureIdentity":
            credential = DefaultAzureCredential()

        endpoint = index_config.vector_database_api_endpoint_configuration.url
        top_n = index_config.vector_database.get("top_n", 10)
        similarity_threshold = index_config.vector_database.get("similarity_threshold", 0.85)

        search_client = SearchClient(endpoint, index_config.vector_database["database_name"], credential)
        vector_query = VectorizedQuery(vector=self.__get_embeddings(query),
                                        k_nearest_neighbors=3,
                                        fields=index_config.vector_database["embedding_property_name"],
                                        threshold=VectorSimilarityThreshold(value=similarity_threshold))

        filter=f"{(index_config.vector_database['vector_store_id_property_name'])} eq '{self.vector_store_name}'"
        if self.file_name:
            filter += f" and {index_config.vector_database['metadata_property_name']}/FileName eq '{self.file_name}'"
        results = search_client.search(
            search_text=query,
            filter=filter,
            vector_queries=[vector_query],
            query_type=self.query_type,
            semantic_configuration_name = self.semantic_configuration_name,
            top=top_n,
            select=[
                "Id",
                index_config.vector_database['content_property_name'],
                index_config.vector_database['metadata_property_name']
            ]
        )

        rerank_available = False

        #load search results into VectorDocument objects for score processing
        for result in results:

            document = VectorDocument(
                    id=result["Id"],
                    page_content=result[index_config.vector_database["content_property_name"]],
                    metadata=result[index_config.vector_database["metadata_property_name"]],
                    score=result["@search.score"],
                    rerank_score=result.get("@search.reranker_score", 0.0)
            )
            if('@search.reranker_score' in result):
                rerank_available = True

            document.score = result["@search.score"]
            search_results.append(document)

        #sort search results by score
        if(rerank_available):
            search_results.sort(key=lambda x: (x.rerank_score, x.score), reverse=True)
        else:
            search_results.sort(key=lambda x: x.score, reverse=True)

        #take top n of search_results
        search_results = search_results[:top_n]

        return search_results

    async def _aget_relevant_documents(
        self, query: str, *, run_manager: AsyncCallbackManagerForRetrieverRun
    ) -> List[Document]:
        """
        Performs an asynchronous hybrid search on Azure AI Search index
        NOTE: This functionality is not currently supported in the underlying Azure SDK.
        """
        raise Exception(f"Asynchronous search not supported.")

    def get_document_content_artifacts(
            self,
            documents: List[Document]) -> List[ContentArtifact]:
        """
        Gets the content artifacts (sources) from the documents retrieved from the retriever.

        Returns:
            List of content artifacts (sources) from the retrieved documents.
        """
        content_artifacts = []
        added_ids = set()  # Avoid duplicates

        for result in documents:  # Unpack the tuple
            result_id = result.id
            metadata = result.metadata
            if metadata is not None and 'multipart_id' in metadata and metadata['multipart_id']:
                if result_id not in added_ids:
                    title = (metadata['multipart_id'][-1]).split('/')[-1]
                    filepath = '/'.join(metadata['multipart_id'])
                    content_artifacts.append(ContentArtifact(id=result_id, title=title, filepath=filepath))
                    added_ids.add(result_id)
        return content_artifacts

    def format_documents(
        self,
        documents:List[Document]) -> str:
        """
        Generates a formatted string from a list of documents for use
        as the context for the completion request.
        """
        return "\n\n".join(doc.page_content for doc in documents)
