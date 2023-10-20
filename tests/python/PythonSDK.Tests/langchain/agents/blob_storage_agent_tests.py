import pytest
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.models.metadata import Agent, DataSource
from foundationallm.models.orchestration import CompletionRequest, MessageHistoryItem
from foundationallm.langchain.agents.blob_storage_agent import BlobStorageAgent

@pytest.fixture
def mock_configuration():
    class MockConfiguration:
        def get_value(self, key: str) -> str:
            return "test"
    return MockConfiguration()

@pytest.fixture
def mock_base_language_model():
    class MockBaseLanguageModel:
        def get_language_model(self):
            return "test"
    return MockBaseLanguageModel()

@pytest.fixture  
def completion_request():  
    req = CompletionRequest(user_prompt="test user prompt")  
    req.agent = Agent(name="test agent", type="test agent type", description="test agent description",   
                      prompt_template="test prompt template")          
    req.data_source = DataSource(name="test ds", type="test ds", description="test ds",   
                                  configuration=BlobStorageConfiguration(  
                                     connection_string_secret="test_connection_string_secret",  
                                     container="test_container", files=["test_file.txt"]))          
    req.message_history = [MessageHistoryItem(sender="User", text="user chat 1"),  
                           MessageHistoryItem(sender="Agent", text="agent chat 1"),  
                           MessageHistoryItem(sender="User", text="user chat 2"),  
                           MessageHistoryItem(sender="Agent", text="agent chat 2")]
    return req

@pytest.fixture
def blob_storage_agent(completion_request, mock_base_language_model, mock_configuration):
    agent = BlobStorageAgent(completion_request=completion_request,llm = mock_base_language_model, config=mock_configuration)
    return agent

class BlobStorageAgentTests:

    def test_build_chat_history_should_use_default_labels(self, completion_request, blob_storage_agent):
        history = blob_storage_agent.build_chat_history(completion_request.message_history)
        expected = "\n\nChat History:\nHuman: user chat 1\nAgent: agent chat 1\nHuman: user chat 2\nAgent: agent chat 2\n\n\n"
        assert history == expected
    
    def test_build_chat_history_should_use_custom_labels(self, completion_request, blob_storage_agent):
        history = blob_storage_agent.build_chat_history(completion_request.message_history, human_label="Bob", ai_label="Sandy")
        expected = "\n\nChat History:\nBob: user chat 1\nSandy: agent chat 1\nBob: user chat 2\nSandy: agent chat 2\n\n\n"
        assert history == expected