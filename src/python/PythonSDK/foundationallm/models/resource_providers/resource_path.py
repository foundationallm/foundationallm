from pydantic import BaseModel, Field
from typing import Self, Optional, ClassVar

class ResourcePath(BaseModel):
    """
    Provides properties associated with a FoundationaLLM resource path.
    """

    INSTANCE_TOKEN: ClassVar[str] = 'instances'
    RESOURCE_PROVIDER_TOKEN: ClassVar[str] = 'providers'

    instance_id: Optional[str] = Field(description="The FoundationaLLM instance identifier.")
    resource_provider: Optional[str] = Field(description="The FoundationaLLM resource provider.")
    main_resource_type: Optional[str] = Field(description="The main resource type of the resource path.")
    main_resource_id: Optional[str] = Field(None, description="The main resource identifier of the resource path.")

    @staticmethod
    def parse(object_id: str) -> Self:
        """
        Parses a resource path string into a ResourcePath object.

        Args:
            resource_path (str): The resource path string to parse.

        Returns:
            ResourcePath: The parsed ResourcePath object.
        """
        resource_path: ResourcePath = None

        parts = object_id.strip("/").split("/")
        if len(parts) < 5 \
            or parts[0] != ResourcePath.INSTANCE_TOKEN \
            or parts[1].strip() == "" \
            or parts[2] != ResourcePath.RESOURCE_PROVIDER_TOKEN \
            or parts[3].strip() == "" \
            or parts[4].strip() == "" :
            raise ValueError("The resource path is invalid.")
        resource_path =  ResourcePath(
            instance_id=parts[1],
            resource_provider=parts[3],
            main_resource_type=parts[4])
        if (len(parts) >= 6 \
            and parts[5].strip() != ""):
            resource_path.main_resource_id = parts[5]

        return resource_path
