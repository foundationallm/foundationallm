import pytest
from typing import List
from unittest.mock import patch
from foundationallm.config import Configuration, Context
from foundationallm.models import AgentHint
from foundationallm.hubs.data_source import DataSourceRepository, DataSourceResolver, DataSourceHubRequest

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def ds_repository(test_config):
    return DataSourceRepository(config=test_config)

@pytest.fixture
def ds_resolver(test_config, ds_repository):
    return DataSourceResolver(repository=ds_repository, config=test_config)

@pytest.fixture
def user_context():    
    return Context(user_identity='{"name": "Test User","user_name": "testuser@foundationallm.ai","upn": "testuser@foundationallm.ai"}')
    
class DataSourceResolverTests:
    """
    DataSourceResolverTests is responsible for testing retrieval of data source
    metadata from storage.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
    
    The following data source need to be configured in the testing environment:
        about-foundationallm
       
    The following private agent needs to be configured in the testing environment:
        user: user_foundationallm.ai
        data source: tictactoe-ds        
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """   
    def test_global_datasource_no_hint(self, ds_resolver):
        ds_hub_request = DataSourceHubRequest(data_sources=["about-foundationallm"])
        ds_hub_response = ds_resolver.resolve(request=ds_hub_request)
        assert ds_hub_response.data_sources[0].name == "about-foundationallm"
        
    def test_agent_hint_flag_enabled_then_return_private_data_source(self, ds_resolver, user_context):
        with patch.object(Configuration, 'get_feature_flag', return_value=True) as mock_method:
            ds_hub_request = DataSourceHubRequest(data_sources=["tictactoe-ds"])
            ds_hub_response = ds_resolver.resolve(request=ds_hub_request,user_context=user_context, hint=AgentHint(name="user_foundationallm.ai", private=True))
            assert ds_hub_response.data_sources[0].name == "tictactoe-ds"
    
    def test_agent_hint_flag_disabled_then_return_no_data_sources(self, ds_resolver, user_context):
        with patch.object(Configuration, 'get_feature_flag', return_value=False) as mock_method:
            ds_hub_request = DataSourceHubRequest(data_sources=["tictactoe-ds"])
            ds_hub_response = ds_resolver.resolve(request=ds_hub_request,user_context=user_context, hint=AgentHint(name="user_foundationallm.ai", private=True))
            assert len(ds_hub_response.data_sources)==0
            