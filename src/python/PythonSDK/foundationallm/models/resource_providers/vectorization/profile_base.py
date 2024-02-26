"""
Classes:
    - SettingsBase: Base settings for a vectorization profile
    - ConfigurationReferencesBase: Base class to hold configuration references for a vectorization profile
    - ProfileBase: Base class to hold vectorization profile information
Description: Base class to hold vectorization profile information
"""
from typing import Optional
from pydantic import BaseModel
from .settings_base import SettingsBase
from .configuration_references_base import ConfigurationReferencesBase

class ProfileBase(BaseModel):
    """
    Base class to hold vectorization profile information.
    """
    name: str
    object_id: Optional[str] = None
    settings: SettingsBase
    configuration_references: ConfigurationReferencesBase
