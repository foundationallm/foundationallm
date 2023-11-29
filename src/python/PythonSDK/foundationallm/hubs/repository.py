from abc import ABC, abstractmethod
from typing import List
from foundationallm.config import Configuration
from foundationallm.hubs import Metadata

class Repository(ABC):
    """The Repository class is responsible for fetching metadata values."""

    def __init__(self, config: Configuration):
        self.config = config

    @abstractmethod
    def get_metadata_values(self, pattern=None) -> List[Metadata]:
        """
        Returns a list of metadata values optionally filtered by a pattern/pattern objects.
        """
        raise NotImplementedError

    @abstractmethod
    def get_metadata_by_name(self, name: str) -> Metadata:
        """
        Returns a single metadata value specifically by name.
        """
        raise NotImplementedError
