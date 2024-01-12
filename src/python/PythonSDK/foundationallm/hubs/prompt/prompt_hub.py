from foundationallm.config import Configuration
from foundationallm.hubs.prompt import PromptResolver, PromptRepository
from foundationallm.hubs import HubBase

class PromptHub(HubBase):
    """The PromptHub class is responsible for resolving prompts."""

    def __init__(self, config=None):

        # initialize config
        if ( config == None):
            self.config = Configuration()
        else:
            self.config = config

        super().__init__( resolver=PromptResolver(PromptRepository(self.config),config=self.config))
