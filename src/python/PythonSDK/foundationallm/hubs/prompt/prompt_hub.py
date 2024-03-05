from foundationallm.config import Configuration
from foundationallm.hubs import HubBase
from foundationallm.hubs.prompt import PromptResolver, PromptRepository

class PromptHub(HubBase):
    """
    The PromptHub class is responsible for resolving prompts.
    """
    def __init__(self, config=None):
        # initialize config
        self.config = config or Configuration()
        super().__init__(resolver=PromptResolver(PromptRepository(self.config), config=self.config))
