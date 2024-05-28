import pytest
from foundationallm.integration.config import Configuration

@pytest.fixture
def test_config():
    return Configuration()

class ConfigurationTests:
    """
    ConfigurationTests is responsible for testing the application configuration functionality .
    
    This is an integration test class and expects the following environment variable to be set:
        FOUNDATIONALLM_APP_CONFIGURATION_URI      
            
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_configuration_retrieves_key(self, test_config):
        setting = test_config.get_value("FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIUrl")
        print(setting)
        assert setting is not None
        
    def test_configuration_retrieves_key_from_keyvault(self, test_config):
        setting = test_config.get_value("FoundationaLLM:APIs:GatekeeperIntegrationAPI:APIKey")
        print(setting)
        assert setting is not None
