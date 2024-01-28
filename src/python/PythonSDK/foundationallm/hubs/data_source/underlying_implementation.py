from enum import Enum

class UnderlyingImplementation(Enum):
    """
    The UnderlyingImplementation enum is used to indicate
        the underlying implementation of a data source.
    """
    SQL = "sql"
    BLOB_STORAGE = "blob-storage"
    SEARCH_SERVICE = "search-service"
    CSV = "csv"
    CXO = "cxo"
