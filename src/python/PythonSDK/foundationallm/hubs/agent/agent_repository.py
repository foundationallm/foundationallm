"""
The AgentRepository is responsible for retrieving agent metadata
"""
from typing import List
from foundationallm.hubs import Repository
from foundationallm.hubs.agent import AgentMetadata, AgentHubStorageManager

class AgentRepository(Repository):
    """
    The AgentRepository is responsible for retrieving data source metadata from storage.
    """
    def get_metadata_values(self, pattern:str=None) -> List[AgentMetadata]:
        """
        Retrieves a list of AgentMetadata objects, optionally filtered by a pattern.

        Parameters
        ----------
        pattern : str
            Pattern to match when seelcting the agent metadata values to retrieve.
            If None or empty, return all Agents.

        Returns
        -------
        List[AgentMetadata]
            A list of agent metadata objects.
        """
        mgr = AgentHubStorageManager(config=self.config)

        if pattern is None:
            pattern = ""

        agent_files = mgr.list_blobs(path=pattern)

        return [AgentMetadata.model_validate_json(
            mgr.read_file_content(agent_file)) for agent_file in agent_files]

    def get_metadata_by_name(self, name: str) -> AgentMetadata:
        """
        Retrieves an agent metadata object by name.

        Parameters
        ----------
        name : str
            The name of the agent metadata to retrieve.

        Returns
        -------
        AgentMetadata
            The agent metadata object associated with the specified agent name.
        """
        mgr = AgentHubStorageManager(config=self.config)
        agent_file = f'{name}.json'
        agent = None
        if mgr.file_exists(agent_file):
            agent = AgentMetadata.model_validate_json(mgr.read_file_content(agent_file))
        return agent
