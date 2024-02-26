"""
Class Name: InternalContextCompletionRequest
Description: Encapsulates the metadata required to complete an internal context orchestration request.
"""
from typing import Optional
from foundationallm.models.metadata import InternalContextAgent
from .completion_request_base import CompletionRequestBase

class InternalContextCompletionRequest(CompletionRequestBase):
    """
    Internal Context completion request.
    """    
    agent: Optional[InternalContextAgent] = None
