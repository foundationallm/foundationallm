from typing import List
from foundationallm.hubs.data_source.data_sources.search_service import SearchServiceDataSourceMetadata

class CXODataSourceMetadata(SearchServiceDataSourceMetadata):

    """Configuration settings for a CXO agent"""
    sources : List[str]
    retriever_mode : str
    company : str
