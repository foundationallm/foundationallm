from typing import Any
import pytest
from foundationallm.config import Configuration
from foundationallm.models.orchestration import CompletionRequest
from foundationallm.models.metadata import Agent
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.langchain.language_models import LanguageModelFactory
from foundationallm.langchain.agents import SummaryAgent

@pytest.fixture
def test_config():
    return Configuration()                         

@pytest.fixture
def test_fllm_completion_request():
    req = CompletionRequest(
        user_prompt="FoundationaLLM is a groundbreaking platform to revolutionize how to build and manage, secure, trustworthy, enterprise-grade Copilots.",
        agent=Agent(
            name="summarizer",
            type="summary",
            description="Useful for summarizing input text based on a set of rules.",
            prompt_prefix="Write a concise two-word summary of the following:\n\"{text}\"\nCONCISE SUMMARY IN TWO WORDS:"
        ),
        language_model=LanguageModel(
            type=LanguageModelType.OPENAI,
            provider=LanguageModelProvider.MICROSOFT,
            temperature=0,
            use_chat=True            
        )
    )
    return req

@pytest.fixture
def test_fllm_llm(test_fllm_completion_request, test_config):
    model_factory = LanguageModelFactory(language_model=test_fllm_completion_request.language_model, config = test_config)
    return model_factory.get_llm()

class SummaryAgentTests:    
   def test_agent_should_return_summary(self, test_fllm_completion_request, test_fllm_llm, test_config):        
        agent = SummaryAgent(completion_request=test_fllm_completion_request,llm=test_fllm_llm, config=test_config)
        completion_response = agent.run(prompt=test_fllm_completion_request.user_prompt)
        print(completion_response.completion)
        assert "copilot" in completion_response.completion.lower()
