from azure.appconfiguration import AzureAppConfigurationClient, ConfigurationSetting
from azure.identity import DefaultAzureCredential
import pytest
from foundationallm.config import Configuration
import os

@pytest.fixture
def test_config():
    return Configuration()

class ConfigurationTests:
    """
    ConfigurationTests is responsible for testing the application configuration functionality.
    
    This is an integration test class and expects the following environment variable to be set:
        FOUNDATIONALLM_APP_CONFIGURATION_URI
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """

    def test_config_setting_change(self):        
        app_config_uri = os.environ['FOUNDATIONALLM_APP_CONFIGURATION_URI']
        client = AzureAppConfigurationClient(app_config_uri, DefaultAzureCredential())
        config_setting = client.add_configuration_setting(ConfigurationSetting(
            key='FoundationaLLM:Test:TestSetting',
            value='Original'
        ))   
        initial_config = Configuration()
        
        config_setting.value = "Changed"        
        client.set_configuration_setting(config_setting)
        updated_config = Configuration()
        
        client.delete_configuration_setting(config_setting.key)
        
        assert ((initial_config.get_value("FoundationaLLM:Test:TestSetting") == "Original") and \
                (updated_config.get_value("FoundationaLLM:Test:TestSetting") == "Changed"))
        
        
        
        
        
        
       
    