import pytest
from foundationallm.config import Configuration
from foundationallm.models import ListOption
from foundationallm.models.orchestration import MessageHistoryItem
from foundationallm.resolvers import GenericResolverAgent

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def message_history():  
    history = [MessageHistoryItem(sender="User", text="What is FoundationaLLM?"),  
                MessageHistoryItem(sender="Agent", text="FoundationaLLM is a platform accelerating the delivery of secure, trustworthy, enteprise copilots.")
            ]
    return history

@pytest.fixture
def options_list():
    return [ListOption(name="Inaccuracy", description="Inaccuracy is one AI risk that FoundationaLLM helps solve."),
                        ListOption(name="Speed", description="Co-pilot development costs are reduced."),
                        ListOption(name="Scalable", description="FoundationaLLM is load balanced across multiple LLM endpoints.")]


class GenericResolverAgentTests:
    """
    GenericResolverAgentTests is responsible for testing the selection of the best-fit
        option(s) from a list to respond to a user prompt.
        
    This is an integration test class and expects the following environment variable to be set:
        foundationallm-app-configuration-uri
        
    This test class also expects a valid Azure credential (DefaultAzureCredential) session.
    """
    def test_build_options_list(self, test_config, message_history, options_list):        
        user_prompt = "Tell me one reason why I should use FoundationaLLM."        
        agent = GenericResolverAgent(user_prompt=user_prompt, message_history=message_history, options_list=options_list, config=test_config)
        options_text = agent.build_options_list(options_list)        
        assert options_text is not None and len(options_text) > 0
        
    def test_run_should_return_two_names(self, test_config, message_history, options_list):
        user_prompt = "Tell me two reasons why I should use FoundationaLLM."        
        agent = GenericResolverAgent(user_prompt=user_prompt, message_history=message_history, options_list=options_list, config=test_config)        
        selection_list = (agent.run(prompt=user_prompt)).completion                      
        assert len(selection_list) == 2
        
    def test_run_should_return_one_name_speed(self, test_config, message_history, options_list):
        user_prompt = "What is the most important reason that I should use FoundationaLLM. I care about money!"        
        agent = GenericResolverAgent(user_prompt=user_prompt, message_history=message_history, options_list=options_list, config=test_config)        
        selection_list = (agent.run(prompt=user_prompt)).completion        
        assert selection_list[0] == "Speed"