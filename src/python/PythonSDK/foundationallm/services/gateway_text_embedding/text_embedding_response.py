"""
Class: TextEmbeddingResponse
Description:  Class representing the result of a text embedding operation.
"""
from pydantic import BaseModel, Field
from typing import Optional, List
from .text_chunk import TextChunk

class TextEmbeddingResponse(BaseModel):
    """
    Class representing the result of a text embedding operation.
    """
    in_progress: bool = Field(..., alias='in_progress')
    failed: bool = Field(..., alias='failed')
    error_message: Optional[str] = Field(None, alias='error_message')
    operation_id: Optional[str] = Field(None, alias='operation_id')
    text_chunks: List[TextChunk] = Field(default_factory=list, alias='text_chunks')
    token_count: int = Field(..., alias='token_count')
