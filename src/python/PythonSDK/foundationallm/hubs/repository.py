"""
Repository base class for the hubs to fetch metadata values.
"""
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
        Retrieves a list of metadata values optionally filtered by a pattern.

        Parameters
        ----------
        pattern : str
            Pattern to match when seelcting the metadata values to retrieve.

        Returns
        -------
        List[Metadata]
            A list of metadata values optionally filtered by the specified pattern.
        """
        raise NotImplementedError

    @abstractmethod
    def get_metadata_by_name(self, name:str) -> Metadata:
        """
        Retrieves a single metadata value specifically by name.

        Parameters
        ----------
        name : str
            The name of the metadata value to return.
            
        Returns
        -------
        Metadata
            Returns a single metadata value specified by name.
        """
        raise NotImplementedError
