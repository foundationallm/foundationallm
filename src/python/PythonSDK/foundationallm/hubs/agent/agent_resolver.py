"""
The AgentResolver class is responsible for resolving a request to a metadata value.
"""
from foundationallm.config import Context
from foundationallm.hubs import Resolver
from foundationallm.hubs.agent import AgentHubRequest, AgentHubResponse
from foundationallm.models import AgentHint

class AgentResolver(Resolver):
    """
    The AgentResolver class is responsible for resolving a request to a metadata value.
    
    If a hint is provided and the FoundationaLLM-AllowAgentHint flag enabled,
        the resolver will return the agent with that name.
    If a hinted agent is not found, return the default agent.
    """
    def resolve(self, request:AgentHubRequest,
                user_context:Context=None, hint:AgentHint=None) -> AgentHubResponse:
        hint_feature_flag_enabled = self.config.get_feature_flag("FoundationaLLM-AllowAgentHint")
        agent_metadata = None
        if hint_feature_flag_enabled and hint is not None:
            path_prefix = None
            if hint.private:
                # build the container prefix based on the user identity
                user_upn = user_context.user_identity.upn.replace("@", "_")
                path_prefix = f"user-profiles/{user_upn}/"
                # override the container prefix for private agent lookup
                self.repository.container_prefix = path_prefix
            agent_metadata = self.repository.get_metadata_by_name(name=hint.name)
            # Reset the container prefix for global agent (if necessary)
            self.repository.container_prefix = None
        if agent_metadata is None:
            agent_metadata = self.repository.get_metadata_by_name("default")
        return AgentHubResponse(agent=agent_metadata)
