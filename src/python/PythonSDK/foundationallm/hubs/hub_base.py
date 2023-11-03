from abc import ABC
from foundationallm.hubs import Resolver
from foundationallm.context import Context

class HubBase(ABC):
    """The HubBase class is responsible for managing and resolving requests."""
    
    def __init__(self, resolver: Resolver):
        self.resolver = resolver       

    def resolve(self, request, user_context:Context=None, hint:str=None):        
        return self.resolver.resolve(request, user_context, hint)
    
    def list(self):
        """
        Returns a lightweight list (containing of name and description) of all configured metadata items.
        """
        return self.resolver.list()