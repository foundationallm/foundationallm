import pytest
from foundationallm.hubs.agent import AgentHub

@pytest.fixture
def agent_hub():
    return AgentHub()
    
class AgentHubTests:
    """
    AgentHubTests is responsible for testing the listing of agents or selection of the best-fit
        agent to respond to a user prompt with the AgentHub acting as the system under test.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_list_method_returns_at_least_one_lightweight_agent(self, agent_hub):
        """
        The lightweight agent consists of a dictionary containing the name and description of the agent ONLY.
        
        While this test is using the AgentHub as the system under test, it is in fact testing the
        HubBase ABC class where the generic list method is implemented.
        """
        agents_list = agent_hub.list()
        assert len(agents_list) > 0
        
    def test_list_method_contains_default_agent_name(self, agent_hub):
        """
        While this test is using the AgentHub as the system under test, it is in fact testing the
        HubBase ABC class where the generic list method is implemented.
        """
        agent_name_list = [x["name"] for x in agent_hub.list()]
        assert "default" in agent_name_list
        