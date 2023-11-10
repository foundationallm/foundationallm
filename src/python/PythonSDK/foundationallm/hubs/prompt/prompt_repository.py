from foundationallm.hubs import Repository
from foundationallm.hubs.prompt import PromptMetadata
from foundationallm.hubs.prompt import PromptHubStorageManager
from typing import List

class PromptRepository(Repository):
    """The PromptRepository class is responsible for fetching available metadata values."""

    def get_metadata_values(self, pattern:str=None) -> List[PromptMetadata]:
        """Not implemented."""
        pass
    
    def get_metadata_by_name(self, name: str) -> PromptMetadata:
        """
        Returns a PromptMetadata object by name.
        
        Parameters
        ----------
        name : str
            The name of the prompt to return, in the format of AgentName.PromptName
            
        Returns
        -------
        PromptMetadata
            Returns a PromptMetadata object containing the prompt name, prompt text,
            and prompt_suffix, if one exists.
        """
        mgr = PromptHubStorageManager(config=self.config)
        prompt_prefix = mgr.read_file_content(name.replace('.', '/') + '.txt')
        prompt_suffix = mgr.read_file_content(name.replace('.', '/') + '_suffix.txt') or None
        return PromptMetadata(name=name, prompt_prefix=prompt_prefix, prompt_suffix=prompt_suffix)

