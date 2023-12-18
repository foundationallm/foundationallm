"""The DataSourceResolver class is responsible for resolving a request to a metadata value."""
from foundationallm.config import Context
from foundationallm.models import AgentHint
from foundationallm.hubs.data_source import DataSourceHubRequest, DataSourceHubResponse
from foundationallm.hubs import Resolver

class DataSourceResolver(Resolver):
    """The DataSourceResolver class is responsible for resolving a request to a metadata value."""
    def resolve(self, request: DataSourceHubRequest,
                user_context:Context=None, hint:AgentHint=None) -> DataSourceHubResponse:
        hint_feature_flag_enabled = self.config.get_feature_flag("FoundationaLLM-AllowAgentHint")
        if hint_feature_flag_enabled and hint is not None:
            path_prefix = None
            if hint.private:
                # build the container prefix based on the user identity
                user_upn = user_context.user_identity.upn.replace("@", "_")
                path_prefix = f"user-profiles/{user_upn}/"
                # override the container prefix for private agent lookup
                self.repository.container_prefix = path_prefix
        response = DataSourceHubResponse(data_sources=
                                     self.repository.get_metadata_values(request.data_sources))
        # reset any container prefix override
        self.repository.container_prefix = None
        return response
