"""
Encapsulates vector database information.
"""
from typing import Optional
from pydantic import BaseModel
from foundationallm.models.resource_providers.configuration import APIEndpointConfiguration

class VectorDatabaseConfiguration(BaseModel):
    """Vector database metadata model."""
    vector_database : dict
    vector_database_api_endpoint_configuration: APIEndpointConfiguration
