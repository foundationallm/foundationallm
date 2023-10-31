from numpy import isin
import pytest
from typing import List
from foundationallm.config import Configuration
from foundationallm.hubs.agent import AgentRepository, AgentResolver

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def agent_repository(test_config):
    return AgentRepository(config=test_config)

@pytest.fixture
def agent_resolver(agent_repository):
    return AgentResolver(repository=agent_repository)
    
class AgentResolverTests:
    """
    AgentResolverTests is responsible for testing the listing of agents or selection of the best-fit
        agent to respond to a user prompt using the AgentResolver as the system under test.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
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
        