from foundationallm.config import Context
from foundationallm.hubs.data_source import DataSourceHubRequest, DataSourceHubResponse
from foundationallm.hubs import Resolver

class DataSourceResolver(Resolver):
    """The DataSourceResolver class is responsible for resolving a request to a metadata value."""
    def resolve(self, request: DataSourceHubRequest, user_context:Context=None, hint:str=None) -> DataSourceHubResponse:        
        return DataSourceHubResponse(data_sources=self.repository.get_metadata_values(request.data_sources))
