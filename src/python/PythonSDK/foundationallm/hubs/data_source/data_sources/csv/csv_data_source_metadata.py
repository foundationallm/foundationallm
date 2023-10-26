from typing import List, Optional
from foundationallm.hubs.data_source import DataSourceMetadata
from .csv_authentication import CSVAuthentication

class CSVDataSourceMetadata(DataSourceMetadata):
    """
    CSVDataSourceMetadata contains the definition and authentication settings for a CSV data source.
    """
    authentication: CSVAuthentication
    
