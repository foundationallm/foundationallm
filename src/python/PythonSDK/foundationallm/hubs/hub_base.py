from abc import ABC

from foundationallm.config import Context
from foundationallm.hubs import Resolver

class HubBase(ABC):
    """The HubBase class is responsible for managing and resolving requests."""

    def __init__(self, resolver: Resolver):
        self.resolver = resolver

    def resolve(self, request, user_context:Context=None):
        return self.resolver.resolve(request, user_context)

    def list(self):
        """
        Returns a lightweight list (containing of name and description) of
            all configured metadata items.
        """
        return self.resolver.list()
