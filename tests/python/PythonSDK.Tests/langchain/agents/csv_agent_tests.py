import pytest
from foundationallm.config import Configuration
from foundationallm.models import ListOption
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource, LanguageModel
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.langchain.language_models import LanguageModelTypes, LanguageModelProviders
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import CSVAgent


@pytest.fixture
def test_config():
    return Configuration()                         

@pytest.fixture
def test_completion_request():
     req = CompletionRequest(
                         user_prompt="How many survey responses are there?",
                         agent=Agent(name="survey-results", type="csv", description="Useful for answering analytical questions about survey responses.", prompt_prefix="You are an analytic agent named Khalil that helps people find information about the results of a survey.\nProvide consise answers that are polite and professional.\nDo not make anything up, only use the data provided from the survey responses.\nAnswer any questions step-by-step.\n"),
                         data_source=DataSource(name="survey-results-csv", type="blob-storage", description="Information about the survey collected by Hargrove.", configuration=BlobStorageConfiguration(connection_string_secret="FoundationaLLM:BlobStorage:ConnectionString", container="hai-source", files = ["surveydata.csv"])),
                         language_model=LanguageModel(type=LanguageModelTypes.OPENAI, provider=LanguageModelProviders.MICROSOFT, temperature=0, use_chat=True),
                         message_history=[]
                         )
     return req


@pytest.fixture
def test_llm(test_completion_request, test_config):
    model_factory = LanguageModelFactory(language_model=test_completion_request.language_model, config = test_config)
    return model_factory.get_llm() 

class CSVAgentTests:
  def test_successful_initialization(self, test_completion_request, test_llm, test_config):
    agent = CSVAgent(completion_request=test_completion_request,llm=test_llm, config=test_config)
    # Note: the test runner can't deal with the iteration on the completion of the agent, so we'll just test the initialization for now.
    #completion_response = agent.run(prompt=test_completion_request.user_prompt)
    #print(completion_response)
    #assert len(completion_response.completion) > 0
    assert agent is not None