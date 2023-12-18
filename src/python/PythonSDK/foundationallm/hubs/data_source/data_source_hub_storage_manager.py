"""
The DataSourceHubStorageManager class is responsible for fetching requested
datasource metadata
"""
from typing import List
from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager

class DataSourceHubStorageManager(BlobStorageManager):
    """
    The DataSourceHubStorageManager class is responsible for fetching available datasource
        values from Azure Blob Storage.
    """
    def __init__(self, prefix:str=None, config: Configuration = None):
        connection_string = config.get_value(
                    "FoundationaLLM:DataSourceHub:StorageManager:BlobStorage:ConnectionString"
                    )
        container_name = config.get_value(
                    "FoundationaLLM:DataSourceHub:DataSourceMetadata:StorageContainer"
                    )
        if prefix is not None:
            container_name = f"{prefix}/{container_name}"
        super().__init__(blob_connection_string=connection_string,
                            container_name=container_name)

    def read_file_content(self, path) -> str:
        return super().read_file_content(path).decode()

    def list_blobs(self):
        blob_list: List[dict] = list(super().list_blobs(path=""))
        blob_names = [blob["name"].split('/')[-1] for blob in blob_list]
        return blob_names
