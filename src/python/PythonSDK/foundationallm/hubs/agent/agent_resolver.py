from foundationallm.hubs.agent import AgentMetadata, AgentHubRequest, AgentHubResponse, agent_metadata
from foundationallm.hubs import Resolver
from typing import List
from foundationallm.context import Context

class AgentResolver(Resolver):
    """
    The AgentResolver class is responsible for resolving a request to a metadata value.
    
    If a hint is provided and the FoundationaLLM-AllowAgentHint flag enabled, the resolver will return the agent with that name.
    If a hinted agent is not found, return the default agent.
    """
    def resolve(self, request:AgentHubRequest, user_context:Context=None, hint:str=None) -> AgentHubResponse:
        hint_feature_flag_enabled = self.config.get_feature_flag("FoundationaLLM-AllowAgentHint")
        agent_metadata = None
        if hint_feature_flag_enabled and hint is not None:
            agent_metadata = self.repository.get_metadata_by_name(hint)            
        if agent_metadata is None:
           agent_metadata = self.repository.get_metadata_by_name("default")
        return AgentHubResponse(agent=agent_metadata)