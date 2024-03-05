"""
The AgentResolver class is responsible for resolving a request to a metadata value.
"""
from foundationallm.config import Context
from foundationallm.hubs import Resolver
from foundationallm.hubs.agent import AgentHubRequest, AgentHubResponse

class AgentResolver(Resolver):
    """
    The AgentResolver class is responsible for resolving a request to a metadata value.
    """
    def resolve(self, request:AgentHubRequest, user_context:Context=None) -> AgentHubResponse:
        agent_metadata = self.repository.get_metadata_by_name('default')
        return AgentHubResponse(agent=agent_metadata)
