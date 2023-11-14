from typing import Any
import pytest
from foundationallm.config import Configuration
from foundationallm.models import ListOption
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource, LanguageModel
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.langchain.language_models import LanguageModelTypes, LanguageModelProviders
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import BlobStorageAgent

@pytest.fixture
def test_config():
    return Configuration()                         

@pytest.fixture
def test_completion_request():
     req = CompletionRequest(
                         user_prompt="Tell me about the Gorillas and what happened with them during the pandemic?",
                         agent=Agent(name="sdzwa", type="blob-storage", description="Provides details about the San Diego Zoo Wildlife Alliance originating from the 2022 and 2023 issues of the journal.", prompt_prefix="You are the San Diego Zoo assistant named Sandy. You are responsible for answering questions related to the San Diego Zoo that is contained in the journal publications. Only answer questions that relate to the Zoo and journal content. Do not make anything up. Use only the data provided."),
                         data_source=DataSource(name="sdzwa", type="blob-storage", description="Information about the San Diego Zoo publications.", configuration=BlobStorageConfiguration(connection_string_secret="FoundationaLLM:BlobStorage:ConnectionString", container="zoo-source", files = ["SDZWA-Journal-July-2022.pdf","SDZWA-Journal-July-2023.pdf","SDZWA-Journal-March-2022.pdf", "SDZWA-Journal-March-2023.pdf", "SDZWA-Journal-May-2022.pdf","SDZWA-Journal-May-2023.pdf", "SDZWA-Journal-November-2022.pdf", "SDZWA-Journal-November-2023.pdf", "SDZWA-Journal-September-2022.pdf","SDZWA-Journal-September-2023.pdf"])),
                         language_model=LanguageModel(type=LanguageModelTypes.OPENAI, provider=LanguageModelProviders.MICROSOFT, temperature=0, use_chat=True),
                         message_history=[]
                         )
     return req


@pytest.fixture
def test_llm(test_completion_request, test_config):
    model_factory = LanguageModelFactory(language_model=test_completion_request.language_model, config = test_config)
    return model_factory.get_llm()

class BlobStorageAgentTests:    
   
# TESTS ARE COMMENTED OUT DUE TO THE LENGTH OF TIME IT TAKES TO VECTORIZE THE PDF FILES
#    def test_agent_should_return_valid_response_gorilla_pandemic_pdf(self, test_completion_request, test_llm, test_config):        
#        agent = BlobStorageAgent(completion_request=test_completion_request,llm=test_llm, config=test_config)
#        completion_response = agent.run(prompt=test_completion_request.user_prompt)
#        print(completion_response.completion)
#        assert "gorillas" in completion_response.completion.lower()
#        
#    def test_agent_should_return_valid_response_journal_versions_pdf(self, test_completion_request, test_llm, test_config):        
#        test_completion_request.user_prompt = "How often is the Journal published?"        
#        agent = BlobStorageAgent(completion_request=test_completion_request,llm=test_llm, config=test_config)
#        completion_response = agent.run(prompt=test_completion_request.user_prompt)
#        print(completion_response.completion)
#        assert "bimonthly" in completion_response.completion.lower()