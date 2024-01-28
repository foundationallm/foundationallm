from typing import List, Literal
from foundationallm.langchain.data_sources import DataSourceConfiguration
from foundationallm.langchain.data_sources.search_service import SearchServiceConfiguration

class CXOConfiguration(SearchServiceConfiguration):

    """Configuration settings for a stock agent"""
    configuration_type: Literal['cxo']
    sources : List[str]
    retriever_mode : str
    company : str
