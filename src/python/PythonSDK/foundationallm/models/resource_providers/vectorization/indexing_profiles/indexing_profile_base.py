"""
Class: IndexingProfileBase
Description: Base class for vectorization index profiles.
"""
from foundationallm.models.resource_providers.vectorization import ProfileBase

class IndexingProfileBase(ProfileBase):
    """
    Base class to hold indexing profile information.
    """
    indexer: str
