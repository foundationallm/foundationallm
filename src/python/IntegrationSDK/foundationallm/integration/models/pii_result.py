"""
The PIIResult class encapsulates the identification
of a PII entity in textual content.
"""
from pydantic import BaseModel

class PIIResult(BaseModel):
    """
    The PIIResult class encapsulates identifies
    the type of PII entity and location in
    the textual content of the original request.
    """
    entity_type: str
    start_index: int
    end_index: int
