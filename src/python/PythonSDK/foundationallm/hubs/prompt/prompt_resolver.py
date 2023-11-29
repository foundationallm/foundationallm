from foundationallm.config import Context
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse
from foundationallm.hubs import Resolver

class PromptResolver(Resolver):
    """The PromptResolver class is responsible for resolving a request to a metadata value."""

    def resolve(self, request:PromptHubRequest, user_context:Context=None,
                hint:str=None) -> PromptHubResponse:
        return PromptHubResponse(prompt=
                                 self.repository.get_metadata_by_name(
                                     request.agent_name + '.' + request.prompt_name)
                                 )
