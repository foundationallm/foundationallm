from pydantic import BaseModel, Field, computed_field
from typing import Optional, Any, Self
from foundationallm.utils import ObjectUtils
from foundationallm.langchain.exceptions import LangChainException
from foundationallm.models.resource_providers import ResourcePath

class ResourceObjectIdProperties(BaseModel):
    """
    Provides properties associated with a FoundationaLLM resource object identifier.
    """
    object_id: str = Field(description="The FoundationaLLM resource object identifier.")
    properties: Optional[dict] = Field(default={}, description="A dictionary containing properties associated with the object identifier.")

    @computed_field
    @property
    def resource_path(self) -> ResourcePath:
        """
        The resource path associated with the resource object identifier.

        Returns:
            ResourcePath: The resource path object.
        """
        result: ResourcePath = None
        try:
            result = ResourcePath.parse(self.object_id)
        except Exception as e:
            raise LangChainException(f"The resource object identifier is invalid. {str(e)}", 400)

        return result
