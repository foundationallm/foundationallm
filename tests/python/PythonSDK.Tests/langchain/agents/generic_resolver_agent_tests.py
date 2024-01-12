import pytest
from foundationallm.config import Configuration
from foundationallm.models import ListOption
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent, DataSource
from foundationallm.langchain.data_sources.blob import BlobStorageConfiguration
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import GenericResolverAgent

@pytest.fixture
def test_config():
    return Configuration()                         

@pytest.fixture
def test_completion_request():
    req = CompletionRequest(
        user_prompt="What is FoundationaLLM?",
        agent=Agent(
            name="demos",
            type="generic-resolver",
            description="Useful for choosing one or more items from a list of FoundationaLLM demo options.",
            prompt_prefix="You are a demo option selector in the FoundationaLLM system. Your job is to select one or more options from a list of options that can best satisfy the incoming User Prompt. An option consists of a name and description. Evaluate each option by its description.\n\nYou are to select the number of options that are requested in the user prompt. Provide the answer in natural language.\n\nExample: The best option for your request is the default demo.\n\nDo not make anything up, do not create a fake conversation, use only the data provided here.\n\n{options}\n\nUser Prompt:{user_prompt}\n\nAnswer:\n"
        ),
        data_sources=[DataSource(
            name="foundationallm-demos",
            type="blob-storage",
            description="Information about FoundationaLLM demos.",
            configuration=BlobStorageConfiguration(
                connection_string_secret="FoundationaLLM:BlobStorageMemorySource:BlobStorageConnection",
                container="demos-source",
                files = ["demos.json"]
            )
        )],
        language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True
        ),
        message_history=[
            MessageHistoryItem(sender="User", text="What is FoundationaLLM?"),  
            MessageHistoryItem(sender="Agent", text="FoundationaLLM is a platform accelerating the delivery of secure, trustworthy, enteprise copilots.")
        ]
    )
    return req


@pytest.fixture
def test_llm(test_completion_request, test_config):
    model_factory = LanguageModelFactory(language_model=test_completion_request.language_model, config = test_config)
    return model_factory.get_llm()    

class GenericResolverAgentTests:
    """
    GenericResolverAgentTests is responsible for testing the selection of the best-fit
        option(s) from a list to respond to a user prompt.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session and
        the file demos.json to be present in the Azure Blob Storage container foundationallm-demo-source.
    """

    def test_read_json_options_from_storage(self, test_completion_request, test_llm, test_config):
       agent = GenericResolverAgent(completion_request=test_completion_request,llm=test_llm, config=test_config)
       print(agent)
       options_list = agent.load_options()
       print(options_list)
       assert len(options_list) > 0
        
    def test_run_should_return_two_names(self, test_completion_request, test_llm, test_config):
        prompt = "Give me two demos that include a demonstration of the RAG pattern that includes data for FoundationaLLM and Solliance."
        user_prompt = prompt
        test_completion_request.user_prompt = prompt
        agent = GenericResolverAgent(completion_request=test_completion_request,llm=test_llm, config=test_config)
        selection_list = (agent.run(prompt=user_prompt)).completion 
        print(selection_list)
        assert 'default' in selection_list.lower() and 'solliance' in selection_list.lower()
        
    def test_run_should_return_one_name_survey_results(self, test_completion_request, test_llm, test_config):
        prompt = "What demo should I use to demonstrate the CSV features of FoundationaLLM?"        
        user_prompt = prompt
        test_completion_request.user_prompt = prompt
        agent = GenericResolverAgent(completion_request=test_completion_request,llm=test_llm, config=test_config)
        selection_list = (agent.run(prompt=user_prompt)).completion        
        print(selection_list)
        assert "hai" in selection_list.lower()        
   