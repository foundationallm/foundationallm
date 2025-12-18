from typing import Optional
from pydantic import Field
from foundationallm.models.resource_providers import ResourceBase
from foundationallm.models.resource_providers.prompts import PromptTypes

class MultipartPrompt(ResourceBase):
    """
    Encapsulates a multipart prompt composed of a prefix and a suffix.
    """
    type: str = Field(default=PromptTypes.MULTIPART, description='The type of the prompt.')
    prefix: Optional[str] = Field(default='', description='The prefix of the prompt.')
    suffix: Optional[str] = Field(default='', description='The suffix of the prompt.')
