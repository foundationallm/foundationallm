"""
Encapsulates properties an agent tool
"""
from typing import Optional
from pydantic import BaseModel, Field

class AgentTool(BaseModel):
    """
    Encapsulates properties for an agent tool.
    """
    name: Optional[bool] = False
    ai_model_object_ids: Optional[dict] = Field(default=[], description="A dictionary object identifiers of the AIModel objects for the agent tool.")
    api_endpoint_configuration_object_ids: Optional[dict] = Field(default=[], description="A dictionary object identifiers of the APIEndpointConfiguration objects for the agent tool.")
