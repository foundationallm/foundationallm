"""
Class Name: SearchServiceDataSourceMetadata
Description: Encapsulates Azure AI Search service index details
"""
from typing import Optional
from foundationallm.hubs.data_source import DataSourceMetadata
from .search_service_authentication_metadata import SearchServiceAuthenticationMetadata

class SearchServiceDataSourceMetadata(DataSourceMetadata):
    """
    Represents search service data source that details authentication, connection,
    and details on the content contained within
    """
    index_name: str
    authentication: SearchServiceAuthenticationMetadata
    embedding_field_name: Optional[str] = "Embedding"
    text_field_name: Optional[str] = "Text"
    top_n: Optional[int] = 3
