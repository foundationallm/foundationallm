"""
Class: TextChunk
Description:  Class representing a text chunk in an embedding vector operation.
"""
from pydantic import BaseModel, Field
from typing import Optional
from .embedding import Embedding

class TextChunk(BaseModel):
    """
    Class representing a text chunk in an embedding vector operation.
    """
    operation_id: Optional[str] = Field(None, alias='operation_id')
    position: int
    content: Optional[str] = None
    embedding: Optional[Embedding] = None
    tokens_count: int = Field(..., alias='tokens_count')
