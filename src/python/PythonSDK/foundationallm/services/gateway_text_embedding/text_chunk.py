"""
Class: TextChunk
Description:  Class representing a text chunk in an embedding vector operation.
"""
from pydantic import BaseModel
from typing import Optional, List

class TextChunk(BaseModel):
    """
    Class representing a text chunk in an embedding vector operation.
    """
    operation_id: Optional[str] = None
    position: Optional[int] = 1 # position is one-based
    content: Optional[str] = None # comes back empty on the reponse
    embedding: Optional[List[float]] = None
    tokens_count: Optional[int] = 0
