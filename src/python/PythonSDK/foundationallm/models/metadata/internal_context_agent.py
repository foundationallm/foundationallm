"""
Class Name: InternalContextAgent

Description: Encapsulates the metadata for the agent
fulfilling the orchestration request. This agent is used
as a pass-through of the prompt directly to the LLM
"""
from .agent_base import AgentBase

class InternalContextAgent(AgentBase):
    """Internal Context Agent metadata model."""
