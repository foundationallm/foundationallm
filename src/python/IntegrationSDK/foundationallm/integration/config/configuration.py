"""
Contains the implementation of the Configuration class that is responsible for resolving
configuration settings from Azure App Configuration.
"""
import os
from azure.identity import DefaultAzureCredential
from azure.appconfiguration.provider import (
    AzureAppConfigurationKeyVaultOptions,
    SettingSelector,
    load
)

# Configuration only has one method by design.
# pylint: disable=too-few-public-methods
class Configuration:
    """
    Configuration class that is responsible for resolving configuration settings
    from Azure App Configuration.
    """
    def get_value(self, key: str) -> str:
        """
        Retrieves the setting value from Azure App Configuration.
        If the value is not found the method raises an exception.

        Parameters
        ----------
        - key : str
            The key name of the configuration setting to retrieve.
        
        Returns
        -------
        The configuration value

        Raises an exception if the configuration value is not found.
        """
        app_config_uri = os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI']
        credential = DefaultAzureCredential(
            exclude_environment_credential=True)
        # Connect to Azure App Configuration with key filter
        selectors = [SettingSelector(
            key_filter="FoundationaLLM:APIs:GatekeeperIntegrationAPI:*")]
        app_config = load(endpoint=app_config_uri, credential=credential, selects=selectors,
                            key_vault_options=
                            AzureAppConfigurationKeyVaultOptions(credential=credential))
        return app_config[key]
