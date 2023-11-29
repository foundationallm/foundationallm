from foundationallm.config import Configuration
from foundationallm.hubs.agent import AgentRepository, AgentResolver
from foundationallm.hubs import HubBase

class AgentHub(HubBase):
    """The AgentHub is responsible for resolving agents."""
    def __init__(self):
        # initialize config
        self.config = Configuration()
        super().__init__(resolver=
                            AgentResolver(repository=
                                    AgentRepository(self.config),
                                        config=self.config))
