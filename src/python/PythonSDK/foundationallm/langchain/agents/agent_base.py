from abc import ABC, abstractmethod

from foundationallm.models.orchestration import CompletionResponse

class AgentBase(ABC):
    """Abstract base class for Agents"""

    @abstractmethod
    def run(self, prompt: str) -> CompletionResponse:
        """
        Execute the agent's run method
        
        Parameters
        ----------
        prompt : str
            The prompt for which a completion is begin generated.

        Returns
        -------
        CompletionResponse
            Returns a response containing the completion plus token usage and cost details.
        """
