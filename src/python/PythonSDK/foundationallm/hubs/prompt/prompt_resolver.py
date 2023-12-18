"""The PromptResolver class is responsible for resolving a request to a metadata value."""
from foundationallm.config import Context
from foundationallm.models import AgentHint
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse
from foundationallm.hubs import Resolver

class PromptResolver(Resolver):
    """The PromptResolver class is responsible for resolving a request to a metadata value."""

    def resolve(self, request:PromptHubRequest, user_context:Context=None,
                hint:AgentHint=None) -> PromptHubResponse:
        hint_feature_flag_enabled = self.config.get_feature_flag("FoundationaLLM-AllowAgentHint")
        if hint_feature_flag_enabled and hint is not None:
            path_prefix = None
            if hint.private:
                # build the container prefix based on the user identity
                user_upn = user_context.user_identity.upn.replace("@", "_")
                path_prefix = f"user-profiles/{user_upn}/"
                # override the container prefix for private agent lookup
                self.repository.container_prefix = path_prefix
        response = PromptHubResponse(prompt=
                                 self.repository.get_metadata_by_name(
                                     request.agent_name + '.' + request.prompt_name)
                                 )
        # reset any container prefix override
        self.repository.container_prefix = None
        return response
