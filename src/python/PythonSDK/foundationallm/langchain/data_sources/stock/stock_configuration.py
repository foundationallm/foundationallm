from typing import List
from foundationallm.langchain.data_sources import DataSourceConfiguration

class StockConfiguration(DataSourceConfiguration):
    """Configuration settings for a stock agent"""
    open_ai_endpoint: str
    open_ai_key: str

    search_endpoint: str
    search_key: str

    connection_string_secret : str
    container : str

    embedding_model : str

    top_n : int

    index_name : str

    sources : List[str]

    retriever_mode : str
    load_mode : str

    company : str
