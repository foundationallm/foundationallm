"""
Repository base class for the hubs to fetch metadata values.
"""
from abc import ABC, abstractmethod
from typing import List
from foundationallm.config import Configuration
from foundationallm.hubs import Metadata

class Repository(ABC):
    """The Repository class is responsible for fetching metadata values."""

    def __init__(self, config: Configuration, container_prefix:str = None):
        self.config = config
        self._prefix = container_prefix

    @property
    def container_prefix(self):
        """
        The container_prefix allows for the override of the container path
        to include a prefix, this is used when retrieving private metadata
        """
        return self._prefix

    @container_prefix.setter
    def container_prefix(self, value:str):
        """
        Setter for the container_prefix property
        """
        self._prefix = value

    @abstractmethod
    def get_metadata_values(self, pattern=None) -> List[Metadata]:
        """
        Returns a list of metadata values optionally filtered by a pattern/pattern objects.
        """
        raise NotImplementedError

    @abstractmethod
    def get_metadata_by_name(self, name:str) -> Metadata:
        """
        Returns a single metadata value specifically by name.
        Arguments:
            name (str): The name of the metadata value to return.
            prefix (str): The prefix to use when fetching the metadata value,
                    this is typically the path to the user's profile container
                    for retrieval of private metadata values. If this value is
                    empty, the global metadata container will be used.
        """
        raise NotImplementedError
