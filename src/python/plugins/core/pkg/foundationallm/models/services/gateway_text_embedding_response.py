"""
Class:GatewayTextEmbeddingResponse
Description:  Class representing the response from the Gateway API service for text embedding.
"""
from typing import List
from pydantic import BaseModel

class GatewayTextEmbeddingResponse(BaseModel):
    """
    Class representing an embedding vector operation result.
    """
    embedding_vector: List[float]
    tokens_count: int
