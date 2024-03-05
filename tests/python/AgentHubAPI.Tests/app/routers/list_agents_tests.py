import pytest
from foundationallm.config import Configuration
from app.main import app
from fastapi.testclient import TestClient

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def api_key(test_config):
    return test_config.get_value("FoundationaLLM:APIs:AgentHubAPI:APIKey")

@pytest.fixture
def headers(api_key):
    return { "X-API-KEY": api_key, "X-USER-IDENTITY": '{"name": "Test User","user_name": "testuser@foundationallm.ai","upn": "testuser@foundationallm.ai"}' }

@pytest.fixture
def client():
    return TestClient(app)

class ListAgentsTests:
    """
    ListAgentTests is responsible for testing API responsible for the selection of the best-fit
        agent to respond to a user prompt.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_invalid_api_key_should_return_401(self, client):
        response = client.get("/list", headers={ "X-API-KEY": "invalid" })
        assert response.status_code == 401
    
    def test_list_agents_returns_values(self, client, headers):         
        response = client.get("/list", headers=headers)
        sut = response.json()        
        assert len(sut) > 0