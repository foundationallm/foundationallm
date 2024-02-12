"""
Class Name: KnowledgeManagementAgent

Description:
    Encapsulates the metadata for the agent
    fulfilling the orchestration request.
"""
from typing import Optional, List
from pydantic import validator
from .agent_base import AgentBase

class KnowledgeManagementAgent(AgentBase):
    """Knowledge Management Agent metadata model."""
    indexing_profiles: Optional[List[str]] = None
    embedding_profile: Optional[str] = None

    @validator('indexing_profiles', pre=True)  
    def convert_single_str_to_list(cls, v):
        """Allows for the support of a single indexing profile string."""
        if isinstance(v, str):  
            return [v]  
        return v 

