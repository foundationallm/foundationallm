from typing import List, Union
from pydantic import BaseModel
from .data_source_metadata import DataSourceMetadata
from .data_sources.sql import SQLDataSourceMetadata
from .data_sources.blob_storage import BlobStorageDataSourceMetadata
from .data_sources.csv import CSVDataSourceMetadata
from .data_sources.cxo import CXODataSourceMetadata
from .data_sources.search_service import SearchServiceDataSourceMetadata

class DataSourceHubResponse(BaseModel):
    data_sources: List[Union[DataSourceMetadata, SQLDataSourceMetadata,
                             BlobStorageDataSourceMetadata, CSVDataSourceMetadata, CXODataSourceMetadata,
                             SearchServiceDataSourceMetadata]]
