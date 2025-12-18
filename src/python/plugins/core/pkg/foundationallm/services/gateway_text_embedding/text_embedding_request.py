"""
Class: TextEmbeddingRequest
Description:  Class representing a request into the gateway text embedding service.
"""
from pydantic import BaseModel
from typing import List
from .text_chunk import TextChunk

class TextEmbeddingRequest(BaseModel):
    """
    Class representing a request into the gateway text embedding service.
    """
    text_chunks: List[TextChunk]
    embedding_model_name: str
    prioritized: bool = False
    embedding_model_dimensions: int = 1536
