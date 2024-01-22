from enum import Enum

class AgentType(str,Enum):
    """ The AgentType enum is used to indicate the type of an agent implimentation to use."""
    CSV = "csv"
    SQL = "sql"
    BLOB_STORAGE = "blob-storage"
    ANOMALY = "anomaly"
    CONVERSATIONAL = "conversational"
    SUMMARY = "summary"
    GENERIC_RESOLVER = "generic-resolver"
    SEARCH_SERVICE = "search-service"
    CXO = "cxo"
