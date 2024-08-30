"""
Class: Embedding
Description:  Class representing an embedding vector operation result.
"""
from typing import List
from pydantic import BaseModel, validator

class Embedding(BaseModel):
    """
    Class representing an embedding vector operation result.
    """
    vector: List[float]
