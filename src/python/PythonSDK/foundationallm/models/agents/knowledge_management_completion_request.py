"""
Class Name: KnowledgeManagementCompletionRequest
Description: Encapsulates the metadata required to complete a knowledge management orchestration request.
"""
from typing import Optional
from .knowledge_management_agent import KnowledgeManagementAgent
from foundationallm.models.orchestration import CompletionRequestBase

class KnowledgeManagementCompletionRequest(CompletionRequestBase):
    """
    The completion request received from the Orchestration API.
    """    
    agent: Optional[KnowledgeManagementAgent] = None
    objects: dict = {}
