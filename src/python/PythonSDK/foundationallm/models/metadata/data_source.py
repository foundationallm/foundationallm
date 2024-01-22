from pydantic import Field
from typing import Optional, Union, Annotated
from foundationallm.langchain.data_sources.csv import CSVConfiguration
from foundationallm.langchain.data_sources.sql import SQLDatabaseConfiguration
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.langchain.data_sources.cxo import CXOConfiguration
from foundationallm.langchain.data_sources.search_service import SearchServiceConfiguration
from .metadata_base import MetadataBase

TypedConfiguration = Annotated[
    Union[
        CSVConfiguration,
        SQLDatabaseConfiguration,
        BlobStorageConfiguration,
        SearchServiceConfiguration,
        CXOConfiguration,
    ],
    Field(discriminator='configuration_type')
]

class DataSource(MetadataBase):
    """Data source metadata model."""
    configuration: TypedConfiguration
    data_description: Optional[str] = None
