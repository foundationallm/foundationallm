from enum import Enum

class AuthenticationTypes(str, Enum):
    """Enumerator of the Authentication providers."""
    UNKNOWN = "Unknown"
    AZURE_IDENTITY = "AzureIdentity"
    API_KEY = "APIKey"
    CONNECTION_STRING = "ConnectionString"
    ACCOUNT_KEY = "AccountKey"
