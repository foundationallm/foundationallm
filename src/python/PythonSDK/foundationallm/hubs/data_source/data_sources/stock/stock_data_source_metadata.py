from typing import List, Optional
from foundationallm.hubs.data_source import DataSourceMetadata

class StockDataSourceMetadata(DataSourceMetadata):
    """Configuration settings for a stock agent"""
    open_ai_endpoint: str
    open_ai_key: str

    search_endpoint: str
    search_key: str

    connection_string_secret : str
    container : str

    embedding_model : str

    config_value_base_name : str

    index_name : str
    sources : List[str]

    retriever_mode : str
    load_mode : str

    company : str
