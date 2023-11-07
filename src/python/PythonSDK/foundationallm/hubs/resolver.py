from abc import ABC, abstractmethod
from .metadata import Metadata
from .repository import Repository
from typing import List

from foundationallm.config import Configuration, Context

class Resolver(ABC):
    """The Resolver class is responsible for resolving a request to a list of metadata value."""
    
    def __init__(self, repository: Repository, config:Configuration=None):
        self.repository = repository        
        self.config = config

        
    def list(self) -> List:
        """
        Returns a lightweight list (containing of name and description) of all configured metadata items.
        """
        all_values = self.repository.get_metadata_values()
        light_weight_list = [{"name":x.name, "description": x.description} for x in all_values]
        return light_weight_list
    
    @abstractmethod
    def resolve(self, request, user_context:Context=None, hint:str=None) -> List[Metadata]:
        pass
    
