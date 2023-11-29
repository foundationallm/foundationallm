from typing import List, Optional
from foundationallm.hubs.data_source import DataSourceMetadata
from .blob_storage_authentication_metadata import BlobStorageAuthenticationMetadata

class BlobStorageDataSourceMetadata(DataSourceMetadata):
    authentication: BlobStorageAuthenticationMetadata
    container: str
    files: Optional[List[str]] = None
