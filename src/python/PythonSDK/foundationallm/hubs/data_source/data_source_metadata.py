from typing import Optional
from foundationallm.hubs import Metadata
from foundationallm.hubs.data_source import UnderlyingImplementation

class DataSourceMetadata(Metadata):
    """The DataSourceMetadata class is used as a common base definition for data source metadata."""
    name: str
    description: str
    underlying_implementation: UnderlyingImplementation
    data_description: Optional[str] = None
