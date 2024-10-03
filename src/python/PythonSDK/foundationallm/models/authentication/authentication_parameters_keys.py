from enum import Enum

class AuthenticationParametersKeys(str, Enum):
    """Enumerator of the Authentication Parameter Keys."""
    API_KEY_CONFIGURATION_NAME = "api_key_configuration_name"
    API_KEY_HEADER_NAME = "api_key_header_name"
    API_KEY_PREFIX = "api_key_prefix"
