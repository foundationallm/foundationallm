""" The DataSourceRepository is responsible for retrieving data source metadata from storage."""
from typing import List
from foundationallm.hubs import Repository
from foundationallm.hubs.data_source import (
        DataSourceMetadata,
        UnderlyingImplementation,
        DataSourceHubStorageManager
    )
from .data_sources.sql import SQLDataSourceMetadata
from .data_sources.blob_storage import BlobStorageDataSourceMetadata
from .data_sources.search_service import SearchServiceDataSourceMetadata
from .data_sources.cxo import CXODataSourceMetadata

class DataSourceRepository(Repository):
    """ The DataSourceRepository is responsible for retrieving data source metadata from storage."""

    def get_metadata_values(self, pattern:List[str]=None) -> List[DataSourceMetadata]:
        """
        Retrieves a list of DataSourceMetadata objects, optionally filtered by a pattern.
        Agents may have multiple datasources defined. In blob storage storage, they are
        stored as JSON files with the naming pattern of datasourcename.json.

        Parameters
        ----------
        pattern : List[str])
            The pattern defines the specific data sources to return (by name).
            If empty or None, return all data sources.

        Returns
        -------
        List[DataSourceMetadata]
            A list of DataSourceMetadata objects, optionally filtered by a pattern.
        """
        mgr = DataSourceHubStorageManager(config=self.config)
        config_files = []
        
        if pattern is None or len(pattern)== 0:
            config_files = mgr.list_blobs()
        else:
            config_files = [datasourcename +".json" for datasourcename in pattern]

        print(config_files)
        configs = []
        for config_file in config_files:
            common_datasource_metadata = {}
            try:
                common_datasource_metadata = DataSourceMetadata.model_validate_json(
                                    mgr.read_file_content(config_file))
                if common_datasource_metadata.underlying_implementation == \
                            UnderlyingImplementation.CXO:
                    configs.append(CXODataSourceMetadata.model_validate_json(
                                    mgr.read_file_content(config_file)))
                if common_datasource_metadata.underlying_implementation == \
                            UnderlyingImplementation.SQL:
                    configs.append(SQLDataSourceMetadata.model_validate_json(
                                    mgr.read_file_content(config_file)))
                elif common_datasource_metadata.underlying_implementation == \
                            UnderlyingImplementation.BLOB_STORAGE:
                    configs.append(BlobStorageDataSourceMetadata.model_validate_json(
                                    mgr.read_file_content(config_file)))
                elif common_datasource_metadata.underlying_implementation == \
                            UnderlyingImplementation.SEARCH_SERVICE:
                    configs.append(SearchServiceDataSourceMetadata.model_validate_json(
                                    mgr.read_file_content(config_file)))
                
            # if a datasource is not deserializable, skip it
            # pylint: disable=bare-except
            except Exception as e:
                print(e)

        return configs

    def get_metadata_by_name(self, name: str) -> DataSourceMetadata:
        """
        Retrieves a DataSourceMetadata object by its name.

        Parameters
        ----------
        name : str
            The name of the data source as well as the name of the configuration file.

        Returns
        -------
        DataSourceMetadata
            A DataSourceMetadata object by name.
        """
        mgr = DataSourceHubStorageManager(config=self.config)
        config_file = name + ".json"
        common_datasource_metadata = DataSourceMetadata.model_validate_json(
                        mgr.read_file_content(config_file))
        config = None
        
        if common_datasource_metadata.underlying_implementation == \
                    UnderlyingImplementation.SQL:
            config = SQLDataSourceMetadata.model_validate_json(
                        mgr.read_file_content(config_file))
        elif common_datasource_metadata.underlying_implementation == \
                UnderlyingImplementation.BLOB_STORAGE:
            config = BlobStorageDataSourceMetadata.model_validate_json(
                        mgr.read_file_content(config_file))
        elif common_datasource_metadata.underlying_implementation == \
                UnderlyingImplementation.SEARCH_SERVICE:
            config = SearchServiceDataSourceMetadata.model_validate_json(
                        mgr.read_file_content(config_file))
        elif common_datasource_metadata.underlying_implementation == UnderlyingImplementation.CSV:
            config = CSVDataSourceMetadata.model_validate_json(mgr.read_file_content(config_file))
        return config
