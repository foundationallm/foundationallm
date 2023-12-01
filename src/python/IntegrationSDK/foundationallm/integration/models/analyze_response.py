"""
The AnalyzeResponse encapsulates the response from the analyzer
with the PII items found in the content.
"""
from typing import List, Union, Optional
from pydantic import BaseModel
from .pii_result import PIIResult
from .pii_result_anonymized import PIIResultAnonymized

class AnalyzeResponse(BaseModel):
    """
    Response object containing textual content as well as
    the PII items found in the content.

    If the intitial request was made with anonymize=True, the response
    will contain PII items with their values replaced with
    placeholders.
    """
    content: str
    results: Optional[List[Union[PIIResult, PIIResultAnonymized]]] = []
