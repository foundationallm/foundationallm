"""
Classes:
    - SettingsBase: Base settings for a vectorization profile
    - ConfigurationReferencesBase: Base class to hold configuration references for a vectorization profile
    - ProfileBase: Base class to hold vectorization profile information
Description: Base class to hold vectorization profile information
"""
from typing import Optional
from foundationallm.models.resource_providers import ResourceBase

class ProfileBase(ResourceBase):
    """
    Base class to hold vectorization profile information.
    """
    settings: Optional[dict] = None
    configuration_references: Optional[dict] = None
