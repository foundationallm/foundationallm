"""
Class: GatewayTextEmbeddingService  
Description:  Class responsible for obtaining text embedding vectors from the Gateway API.
"""
import asyncio
from time import time
from foundationallm.config import Configuration, UserIdentity
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration
from foundationallm.models.services import GatewayTextEmbeddingResponse
from foundationallm.services import HttpClientService
from .text_chunk import TextChunk
from .text_embedding_request import TextEmbeddingRequest
from .text_embedding_response import TextEmbeddingResponse

class GatewayTextEmbeddingService():
    """
    Class for obtaining embedding vectors from the Gateway API.
    """
    def __init__(self,
                 instance_id:str,
                 user_identity:UserIdentity,
                 gateway_api_endpoint_configuration: APIEndpointConfiguration,
                 model_name:str,
                 config: Configuration):
        self.http_client = HttpClientService(gateway_api_endpoint_configuration, user_principal_name, config)       
        self.model_name = model_name
        self.config = config
        self.url =  f'instances/{instance_id}/embeddings'
        
    def get_embedding(self, text: str) -> GatewayTextEmbeddingResponse:
        """
        Get the embedding vector for a given text.
        """              
        # create the text embedding request
        text_chunk = TextChunk(position=0, content=text, tokens_count=0)
        text_embedding_request = TextEmbeddingRequest(text_chunks=[text_chunk], embedding_model_name=self.model_name, prioritized=True)
        # start the operation
        response = TextEmbeddingResponse.model_validate_json(self.http_client.post(self.url, data=text_embedding_request))
        # poll until completion
        while response.in_progress and not response.failed:
            # delay for a second in between polling
            time.sleep(1)
            response = TextEmbeddingResponse.model_validate_json(self.http_client.get(self.url + f'?operationId={response.operation_id}'))

        if response.failed:
            raise Exception(f"Text embedding operation failed: {response.error_message}")

        return GatewayTextEmbeddingResponse(
            embedding_vector=response.text_chunks[0].embedding.vector,
            tokens_count=response.text_chunks[0].tokens_count)

    async def aget_embedding(self, text: str) -> GatewayTextEmbeddingResponse:
        """
        Asynchronously get the embedding vector for a given text.
        """
        text_chunk = TextChunk(position=0, content=text, tokens_count=0)
        text_embedding_request = TextEmbeddingRequest(text_chunks=[text_chunk], embedding_model_name=self.model_name, prioritized=True)
        
        # Send asynchronous POST request to start the operation
        response = TextEmbeddingResponse.model_validate_json(await self.http_client.apost(self.url, data=text_embedding_request))
        
        # Poll until operation is complete
        while response.in_progress and not response.failed:
            await asyncio.sleep(1)  # Use asyncio.sleep for non-blocking delay
            response = TextEmbeddingResponse.model_validate_json(await self.http_client.aget(self.url + f'?operationId={response.operation_id}'))
        
        if response.failed:
            raise Exception(f"Text embedding operation failed: {response.error_message}")

        return GatewayTextEmbeddingResponse(
            embedding_vector=response.text_chunks[0].embedding.vector,
            tokens_count=response.text_chunks[0].tokens_count)
