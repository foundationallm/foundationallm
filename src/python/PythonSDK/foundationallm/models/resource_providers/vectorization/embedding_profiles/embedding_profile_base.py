"""
Class: EmbeddingProfileBase
Description: Base class for vectorization embedding profiles.
"""
from foundationallm.models.resource_providers.vectorization import ProfileBase

class EmbeddingProfileBase(ProfileBase):
    """
    Base class to hold indexing profile information.
    """
    text_embedding: str
