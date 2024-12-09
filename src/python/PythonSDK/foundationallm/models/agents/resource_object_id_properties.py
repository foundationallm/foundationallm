from pydantic import BaseModel, Field, computed_field
from typing import Optional, Any, Self, ClassVar
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.resource_providers import ResourcePath

class ResourceObjectIdProperties(BaseModel):
    """
    Provides properties associated with a FoundationaLLM resource object identifier.
    """
    object_id: str = Field(description="The FoundationaLLM resource object identifier.")
    resource_path: Optional[ResourcePath] = Field(None, description="The resource path object.")
    properties: Optional[dict] = Field(default={}, description="A dictionary containing properties associated with the object identifier.")

    def __init__(self, /, **data: Any) -> None:
        super().__init__(**data)
        self.resource_path = ResourcePath.parse(self.object_id)

