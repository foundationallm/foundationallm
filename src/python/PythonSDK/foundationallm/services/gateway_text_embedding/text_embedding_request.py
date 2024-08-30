"""
Class: TextEmbeddingRequest
Description:  Class representing a request into the gateway text embedding service.
"""
from pydantic import BaseModel, Field
from typing import List
from .text_chunk import TextChunk

class TextEmbeddingRequest(BaseModel):
    """
    Class representing a request into the gateway text embedding service.
    """
    text_chunks: List[TextChunk] = Field(..., alias='text_chunks')
    embedding_model_name: str = Field('', alias='embedding_model_name')
    prioritized: bool = Field(False, alias='prioritized')
