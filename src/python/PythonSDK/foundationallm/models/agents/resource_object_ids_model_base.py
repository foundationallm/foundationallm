from pydantic import BaseModel, Field
from typing import Dict, List
from .resource_object_id_properties import ResourceObjectIdProperties

class ResourceObjectIdsModelBase(BaseModel):
    """
    The base model class for all models that contain a dictionary of resource object identifier properties.
    """

    resource_object_ids: Dict[str, ResourceObjectIdProperties] = Field(default_factory=dict, alias="resource_object_ids")

    def get_resource_object_id_properties(
            self,
            resource_provider_name: str,
            resource_type_name: str,
            property_name: str,
            property_value: str) -> ResourceObjectIdProperties:

        """
        Gets the resource object identifier properties for a specific resource provider, resource type, and object role.

        Args:
            resource_provider_name (str): The resource provider name.
            resource_type_name (str): The resource type name.
            object_role (str): The object role.

        Returns:
            ResourceObjectIdProperties: The resource object identifier properties.
        """
        return next(
            (v for v in self.resource_object_ids.values() \
                if v.resource_path.resource_provider == resource_provider_name \
                    and v.resource_path.main_resource_type == resource_type_name \
                        and property_name in v.properties \
                            and v.properties[property_name] == property_value), None)

    def get_many_resource_object_id_properties(
        self,
        resource_provider_name: str,
        resource_type_name: str,
        property_name: str,
        property_value: str) -> List[ResourceObjectIdProperties]:

        """
        Gets the resource object identifier properties for a specific resource provider, resource type, and object role.

        Args:
            resource_provider_name (str): The resource provider name.
            resource_type_name (str): The resource type name.
            object_role (str): The object role.

        Returns:
            List[ResourceObjectIdProperties]: A list of resource object identifier properties.
        """
        return [
            v for v in self.resource_object_ids.values() \
                if v.resource_path.resource_provider == resource_provider_name \
                    and v.resource_path.main_resource_type == resource_type_name \
                        and property_name in v.properties \
                            and v.properties[property_name] == property_value
        ]
