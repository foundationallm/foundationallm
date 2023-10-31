from abc import ABC
from foundationallm.hubs import Resolver

class HubBase(ABC):
    """The HubBase class is responsible for managing and resolving requests."""
    
    def __init__(self, resolver: Resolver):
        self.resolver = resolver       

    def resolve(self, request):        
        return self.resolver.resolve(request)
    
    def list(self):
        """
        Returns a lightweight list (containing of name and description) of all configured metadata items.
        """
        return self.resolver.list()