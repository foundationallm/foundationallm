"""
The PromptHubStorageManager class is responsible for fetching available
    prompt values from Azure Blob Storage.
"""
from typing import List
from foundationallm.config import Configuration
from foundationallm.storage import BlobStorageManager

class PromptHubStorageManager(BlobStorageManager):
    """
    The PromptHubStorageManager class is responsible for fetching available
        prompt values from Azure Blob Storage.
    """
    def __init__(self, prefix:str=None, config: Configuration = None):
        connection_string = config.get_value(
                        "FoundationaLLM:PromptHub:StorageManager:BlobStorage:ConnectionString"
                        )
        container_name = config.get_value(
                        "FoundationaLLM:PromptHub:PromptMetadata:StorageContainer"
                        )
        if prefix is not None:
            container_name = f"{prefix}/{container_name}"
        super().__init__(blob_connection_string=connection_string,
                            container_name=container_name)

    def read_file_content(self, path) -> str:
        file_content = super().read_file_content(path)
        if file_content is not None:
            return file_content.decode()

    def list_blobs(self, path):
        blob_list: List[dict] = list(super().list_blobs(path=path))
        blob_names = [blob["name"] for blob in blob_list]
        return blob_names
