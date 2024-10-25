from datetime import datetime
from pydantic import Field
from typing import Optional
from foundationallm.models.resource_providers import ResourceName

class ResourceBase(ResourceName):
    """
    Base class containing shared properties for all resources.
    """
    object_id: Optional[str] = Field(default=None, description="The unique identifier of the resource.")
    display_name: Optional[str] = Field(default=None, description="The display name of the resource.")
    description: Optional[str] = Field(default=None, description="The description of the resource.")
    cost_center: Optional[str] = Field(default=None, description="The cost center of the resource.")
    properties: Optional[dict] = Field(default={}, description="The properties of the resource.")
    created_by: Optional[str] = Field(default=None, description="The entity who created the resource.")
    created_on: Optional[datetime] = Field(default=None, description="The date and time the resource was created.")
    updated_by: Optional[str] = Field(default=None, description="The entity who last updated the resource.")
    updated_on: Optional[datetime] = Field(default=None, description="The date and time the resource was last updated.")
    deleted: bool = Field(default=False, description="Indicates if the resource has been logically deleted.")
    expiration_date: Optional[datetime] = Field(default=None, description="The date and time the resource expires.")
    
