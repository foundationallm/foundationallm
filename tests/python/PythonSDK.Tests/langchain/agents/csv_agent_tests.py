import pytest
from foundationallm.config import Configuration
from foundationallm.models import ListOption
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.models.language_models import EmbeddingModel, LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import CSVAgent


@pytest.fixture
def test_config():
    return Configuration()                         

@pytest.fixture
def test_completion_request():
     req = CompletionRequest(
        user_prompt="How many survey responses are there?",
        agent=Agent(
           name="survey-results",
           type="csv",
           description = "Useful for analyzing data in a CSV file.",
           prompt_prefix = "You are an analytics agent named Khalil.\nYou help users answer questions about work from home survey data. If the user asks you to answer any other question besides questions about the data, politely suggest that go ask a human as you are a very focused agent.\nYou are working with a pandas dataframe in Python. The name of the dataframe is `df`.\nYou should use the tools below to answer the question posed of you:",
           prompt_suffix = "This is the result of `print(df.head())`:\n{df_head}\n\nBegin!\n\n{chat_history}\nQuestion: {input}\n{agent_scratchpad}"
        ),
        data_sources=[DataSource(
           name="hai-ds",
           type="csv",
           description= "Useful for when you need to answer questions about survey data.",
           data_description = "Survey data",
           configuration=BlobStorageConfiguration(
              connection_string_secret="FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection",
              container="hai-source",
              files = ["surveydata.csv"]
           )
        )],
        language_model = LanguageModel(
           type=LanguageModelType.OPENAI,
           provider=LanguageModelProvider.MICROSOFT,
           temperature=0,
           use_chat=True
        ),
        embedding_model = EmbeddingModel(
            type = LanguageModelType.OPENAI,
            provider = LanguageModelProvider.MICROSOFT,
            deployment = 'embeddings',
            model = 'text-embedding-ada-002',
            chunk_size = 10
        ),
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