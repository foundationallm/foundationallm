"""
Class Name: KnowledgeManagementCompletionRequest
Description: Encapsulates the metadata required to complete a knowledge management orchestration request.
"""
from typing import Optional

from foundationallm.models.orchestration import CompletionRequestBase

from .agent_base import AgentBase

class CompletionRequest(CompletionRequestBase):
    """
    The completion request received from the Orchestration API.
    """
    agent: Optional[AgentBase] = None
    objects: dict = {}
