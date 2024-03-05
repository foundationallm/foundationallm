"""
The PromptResolver class is responsible for resolving a request to a metadata value.
"""
from foundationallm.config import Context
from foundationallm.hubs import Resolver
from foundationallm.hubs.prompt import PromptHubRequest, PromptHubResponse

class PromptResolver(Resolver):
    """
    The PromptResolver class is responsible for resolving a request to a metadata value.
    """
    def resolve(self, request:PromptHubRequest, user_context:Context=None) -> PromptHubResponse:
        prompt_container = request.prompt_container or 'default'
        prompt = self.repository.get_metadata_by_name(f'{prompt_container}.{request.prompt_name}')
        return PromptHubResponse(prompt=prompt)
