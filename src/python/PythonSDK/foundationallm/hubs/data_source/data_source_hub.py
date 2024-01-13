from foundationallm.config import Configuration
from foundationallm.hubs import HubBase
from foundationallm.hubs.data_source import DataSourceRepository, DataSourceResolver

class DataSourceHub(HubBase):
    """The DataSourceHub is responsible for resolving data sources."""

    def __init__(self, config=None):

        # initialize config
        if ( config == None):
            self.config = Configuration()
        else:
            self.config = config

        super().__init__(resolver= DataSourceResolver(
            repository=DataSourceRepository(config=self.config), config=self.config)
             )
