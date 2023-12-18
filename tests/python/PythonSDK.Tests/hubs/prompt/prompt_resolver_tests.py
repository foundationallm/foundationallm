import pytest
from typing import List
from unittest.mock import patch
from foundationallm.config import Configuration, Context
from foundationallm.models import AgentHint
from foundationallm.hubs.prompt import PromptRepository, PromptResolver, PromptHubRequest

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def prompt_repository(test_config):
    return PromptRepository(config=test_config)

@pytest.fixture
def prompt_resolver(test_config, prompt_repository):
    return PromptResolver(repository=prompt_repository, config=test_config)

@pytest.fixture
def user_context():    
    return Context(user_identity='{"name": "Test User","user_name": "testuser@foundationallm.ai","upn": "testuser@foundationallm.ai"}')
    
class PromptResolverTests:
    """
    PromptResolverTests is responsible for testing retrieval of prompt
    metadata from storage.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
    
    The following agent prompt folder need to be configured in the testing environment:
        default
       
    The following private agent prompts needs to be configured in the testing environment:
        user: user_foundationallm.ai
        prompt: default        
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """   
    def test_global_prompt_no_hint_returns_requested_prompt(self, prompt_resolver):
        prompt_hub_request = PromptHubRequest(agent_name="default")
        prompt_hub_response = prompt_resolver.resolve(request=prompt_hub_request)
        print(prompt_hub_response)
        assert prompt_hub_response.prompt.name == "default.default"
        
    def test_agent_hint_flag_enabled_then_return_private_prompt(self, prompt_resolver, user_context):
        with patch.object(Configuration, 'get_feature_flag', return_value=True) as mock_method:
            prompt_hub_request = PromptHubRequest(agent_name="tictactoe")
            prompt_hub_response = prompt_resolver.resolve(request=prompt_hub_request,user_context=user_context, hint=AgentHint(name="doesnt_matter", private=True))
            print(prompt_hub_response)
            assert prompt_hub_response.prompt.name == "tictactoe.default"
    
    def test_agent_hint_flag_disabled_request_private_raises_exception(self, prompt_resolver, user_context):
        with patch.object(Configuration, 'get_feature_flag', return_value=False) as mock_method:
            prompt_hub_request = PromptHubRequest(agent_name="tictactoe")
            with pytest.raises(ValueError) as e:
                prompt_hub_response = prompt_resolver.resolve(request=prompt_hub_request,user_context=user_context, hint=AgentHint(name="doesnt_matter", private=True))                
            assert str(e.value) == "Prompt 'tictactoe.default' not found."
      