from enum import Enum

class ConfigurationTypes(str, Enum):
    """
    Enum for configuration types.
    """
    BASIC = "basic"
    API_ENDPOINT = "api-endpoint"
    APP_CONFIGURATION_KEY_VALUE = "appconfiguration-key-value"
    APP_CONFIGURATION_KEY_VAULT_REFERENCE = "appconfiguration-key-vault-reference"
