"""
Class: GatewayTextEmbeddingService
Description:  Class responsible for obtaining text embedding vectors from the Gateway API.
"""
import asyncio
import time
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
                 config: Configuration,
                 model_dimensions:int = 1536):
        self.http_client = HttpClientService(gateway_api_endpoint_configuration, user_identity, config)
        self.model_name = model_name
        self.config = config
        self.url =  f'/instances/{instance_id}/embeddings'
        self.model_dimensions = model_dimensions

    def get_embedding(self, text: str) -> GatewayTextEmbeddingResponse:
        """
        Get the embedding vector for a given text.
        """

        if (text == None) or (len(text) == 0):
            raise Exception("Text is empty or None")

        # create the text embedding request
        text_embedding_request = self._create_text_embedding_request(text)

        # start the operation
        request_json = text_embedding_request.model_dump_json(by_alias=True)

        resp = self.http_client.post(self.url, data=request_json)
        response = TextEmbeddingResponse.model_validate(resp)
        # poll until completion
        while response.in_progress and not response.failed:
            # delay for a second in between polling
            time.sleep(1)
            get_resp = self.http_client.get(self.url + f'?operationId={response.operation_id}')
            response = TextEmbeddingResponse.model_validate(get_resp)

        if response.failed:
            raise Exception(f"Text embedding operation failed: {response.error_message}")

        return GatewayTextEmbeddingResponse(
            embedding_vector=response.text_chunks[0].embedding,
            tokens_count=response.text_chunks[0].tokens_count)

    async def aget_embedding(self, text: str) -> GatewayTextEmbeddingResponse:
        """
        Asynchronously get the embedding vector for a given text.
        """

        text_embedding_request = self._create_text_embedding_request(text)

        # start the operation
        request_json = text_embedding_request.model_dump_json(by_alias=True)

        # Send asynchronous POST request to start the operation
        response = TextEmbeddingResponse.model_validate(await self.http_client.post_async(self.url, data=request_json))

        # Poll until operation is complete
        while response.in_progress and not response.failed:
            await asyncio.sleep(1)  # Use asyncio.sleep for non-blocking delay
            response = TextEmbeddingResponse.model_validate(await self.http_client.get_async(self.url + f'?operationId={response.operation_id}'))

        if response.failed:
            raise Exception(f"Text embedding operation failed: {response.error_message}")

        return GatewayTextEmbeddingResponse(
            embedding_vector=response.text_chunks[0].embedding,
            tokens_count=response.text_chunks[0].tokens_count)

    def _create_text_embedding_request(self, text: str) -> TextEmbeddingRequest:
        text_chunk = TextChunk(content=text)
        return TextEmbeddingRequest(text_chunks=[text_chunk], embedding_model_name=self.model_name, embedding_model_dimensions=self.model_dimensions, prioritized=True)
