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

class ResolveTests:
    """
    ResolveTests is responsible for testing API responsible for the selection of the best-fit
        agent to respond to a user prompt.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    These tests also expect the following feature flag to be set and enabled:
        FoundationaLLM-AllowAgentHint
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_invalid_api_key_should_return_401(self, client):
        response = client.post("/resolve", headers={ "X-API-KEY": "invalid" })
        assert response.status_code == 401
        
    def test_x_agent_hint_should_return_desired_agent(self, client, headers):
        headers["X-AGENT-HINT"] = "weather"
        response = client.post("/resolve", headers=headers, json={"user_prompt": "Tell me about FoundationaLLM?"})                
        assert response.json()["agent"]["name"] == "weather"
        
    def test_if_x_agent_hint_does_not_exist_should_return_default_agent(self, client, headers):
        headers["X-AGENT-HINT"] = "invalid"
        response = client.post("/resolve", headers=headers, json={"user_prompt": "Tell me about FoundationaLLM?"})                
        assert response.json()["agent"]["name"] == "default"
    
    def test_if_x_agent_hint_is_empty_should_return_default_agent(self, client, headers):
        headers["X-AGENT-HINT"] = ""
        response = client.post("/resolve", headers=headers, json={"user_prompt": "Tell me about FoundationaLLM?"})                
        assert response.json()["agent"]["name"] == "default"
        