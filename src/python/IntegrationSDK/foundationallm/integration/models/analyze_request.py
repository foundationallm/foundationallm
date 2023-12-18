"""
The AnalyzeRequest class encapsulates the information
required for the analyzer to analyze text content for PII.
"""
from typing import Optional
from pydantic import BaseModel

class AnalyzeRequest(BaseModel):
    """
    Request object to analyze text content for PII.
    Set anonymize to True to replace PII with a placeholder.
    """
    content: str
    anonymize: bool = False
    language: Optional[str] = "en"
