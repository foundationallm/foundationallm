"""
Class: TextEmbeddingResponse
Description:  Class representing the result of a text embedding operation.
"""
from pydantic import BaseModel
from typing import Optional, List
from .text_chunk import TextChunk

class TextEmbeddingResponse(BaseModel):
    """
    Class representing the result of a text embedding operation.
    """
    in_progress: bool
    failed: Optional[bool] = False
    error_message: Optional[str] = None
    operation_id: Optional[str] = None
    text_chunks: Optional[List[TextChunk]] = None
    token_count: Optional[int] = None
