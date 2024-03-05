from foundationallm.config import Configuration
from foundationallm.hubs import HubBase
from foundationallm.hubs.agent import AgentRepository, AgentResolver

class AgentHub(HubBase):
    """
    The AgentHub is responsible for resolving agents.
    """
    def __init__(self, config=None):
        # initialize config
        self.config = config or Configuration()
        super().__init__(resolver=AgentResolver(repository=AgentRepository(self.config), config=self.config))
