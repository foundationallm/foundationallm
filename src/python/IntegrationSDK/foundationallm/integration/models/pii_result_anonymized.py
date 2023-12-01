"""
The PIIResultAnonymized class encapsulates the identification
of a PII entity in textual content along with the anonymized
token and the operator that was used to anonymize the text.
"""
from .pii_result import PIIResult

class PIIResultAnonymized(PIIResult):
    """
    The PIIResultAnonymized class encapsulates identifies
    the type of PII entity and location in
    the textual content with the PII anonymized from the
    original request.
    """
    anonymized_text: str
    operator: str
