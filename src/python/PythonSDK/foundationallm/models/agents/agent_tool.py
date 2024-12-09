"""
Encapsulates properties an agent tool
"""
from typing import Optional, Dict
from pydantic import BaseModel, Field
from .resource_object_id_properties import ResourceObjectIdProperties

class AgentTool(BaseModel):
    """
    Encapsulates properties for an agent tool.
    """
    name: str = Field(..., description="The name of the agent tool.")
    description: str = Field(..., description="The description of the agent tool.")
    package_name: str = Field(..., description="The package name of the agent tool. For internal tools, this value will be FoundationaLLM. For external tools, this value will be the name of the package.")
    resource_object_ids: Dict[str, ResourceObjectIdProperties] = Field(default_factory=dict, description="A dictionary of resource object identifier properties.")
    properties: Optional[dict] = Field(default=[], description="A dictionary of properties for the agent tool.")
