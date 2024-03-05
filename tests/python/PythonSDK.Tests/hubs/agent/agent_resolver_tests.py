import pytest
from typing import List
from unittest.mock import patch
from foundationallm.config import Configuration
from foundationallm.models import AgentHint
from foundationallm.hubs.agent import AgentRepository, AgentResolver, AgentHubRequest

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def agent_repository(test_config):
    return AgentRepository(config=test_config)

@pytest.fixture
def agent_resolver(test_config, agent_repository):
    return AgentResolver(repository=agent_repository, config=test_config)
    
class AgentResolverTests:
    """
    AgentResolverTests is responsible for testing the listing of agents or selection of the best-fit
        agent to respond to a user prompt using the AgentResolver as the system under test.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
    
    The following agents need to be configured in the testing environment:
        default
        solliance
        
    The following private agent needs to be configured in the testing environment:
        user: user_foundationallm.ai
        agent: tictactoe        
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_list_method_returns_at_least_one_lightweight_agent(self, agent_resolver):
        """
        The lightweight agent consists of a dictionary containing the name and description of the agent ONLY.
        
        While this test is using the AgentResolver as the system under test, it is in fact testing the
        Resolver ABC class where the generic list method is implemented.
        """
        agents_list = agent_resolver.list() 
        assert len(agents_list) > 0
        
    def test_list_method_contains_default_agent_name(self, agent_resolver):
        """
        While this test is using the AgentResolver as the system under test, it is in fact testing the
        Resolver ABC class where the generic list method is implemented.
        """
        agent_name_list = [x["name"] for x in agent_resolver.list()]
        assert "default" in agent_name_list
        
    def test_agent_hint_flag_disabled_then_return_default_agent(self):
        with patch.object(Configuration, 'get_feature_flag', return_value=False) as mock_method:
            config = Configuration()
            agent_resolver = AgentResolver(repository=AgentRepository(config=config), config=config)
            agent_hub_request = AgentHubRequest(user_prompt="Tell me about FoundationaLLM?")
            agent_hub_response = agent_resolver.resolve(request=agent_hub_request,hint=AgentHint(name="solliance", private=False))
            assert agent_hub_response.agent.name == "default"
            
    def test_agent_hint_flag_enabled_then_return_hinted_agent(self):
        with patch.object(Configuration, 'get_feature_flag', return_value=True) as mock_method:
            config = Configuration()
            agent_resolver = AgentResolver(repository=AgentRepository(config=config), config=config)
            agent_hub_request = AgentHubRequest(user_prompt="Tell me about FoundationaLLM?")
            agent_hub_response = agent_resolver.resolve(request=agent_hub_request,hint=AgentHint(name="solliance", private=False))
            assert agent_hub_response.agent.name == "solliance"
    
    def test_agent_returns_full_language_and_embedding_model(self):
        with patch.object(Configuration, 'get_feature_flag', return_value=True) as mock_method:
            config = Configuration()
            agent_resolver = AgentResolver(repository=AgentRepository(config=config), config=config)
            agent_hub_request = AgentHubRequest(user_prompt="Tell me about FoundationaLLM?")
            agent_hub_response = agent_resolver.resolve(request=agent_hub_request,hint=AgentHint(name="test-only", private=False))
            assert agent_hub_response.agent.language_model is not None
            assert agent_hub_response.agent.language_model.api_endpoint == "FoundationaLLM:AzureOpenAI:API:Endpoint"
            assert agent_hub_response.agent.language_model.api_version == "FoundationaLLM:AzureOpenAI:API:Version"
            assert agent_hub_response.agent.language_model.api_key == "FoundationaLLM:AzureOpenAI:API:Key"
            assert agent_hub_response.agent.language_model.version == "FoundationaLLM:AzureOpenAI:API:Completions:ModelVersion"
            assert agent_hub_response.agent.language_model.deployment == "FoundationaLLM:AzureOpenAI:API:Completions:DeploymentName"
            assert agent_hub_response.agent.embedding_model is not None
            assert agent_hub_response.agent.embedding_model.api_endpoint == "FoundationaLLM:AzureOpenAI:API:Endpoint"
            assert agent_hub_response.agent.embedding_model.api_version == "FoundationaLLM:AzureOpenAI:API:Version"
            assert agent_hub_response.agent.embedding_model.api_key == "FoundationaLLM:AzureOpenAI:API:Key"
            