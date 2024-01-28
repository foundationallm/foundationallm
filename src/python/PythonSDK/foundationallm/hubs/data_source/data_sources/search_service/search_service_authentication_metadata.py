"""
Class name: SearchServiceAuthenticationMetadata
Description: Encapsulates Azure AI Search Service connection and authentication details
"""
from foundationallm.hubs import Metadata

class SearchServiceAuthenticationMetadata(Metadata):
    """Azure AI Search service connection and authentication details"""
    endpoint: str
    key_secret: str
