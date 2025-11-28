import pytest
from foundationallm.config import Configuration, Context, UserIdentity
from foundationallm.models.metadata import KnowledgeManagementAgent as KnowledgeManagementAgentMetadata
from foundationallm.models.metadata import ConversationHistory, Gatekeeper
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.models.agents import KnowledgeManagementCompletionRequest
from foundationallm.langchain.agents import KnowledgeManagementAgent
from foundationallm.langchain.orchestration import OrchestrationManager

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def test_context():
    return Context(user_identity='{"name": "test", "user_name": "test@foundationallm.ai" , "upn": "test@foundationallm.ai"}')

@pytest.fixture
def test_azure_ai_search_service_completion_request():
     req = KnowledgeManagementCompletionRequest(
         user_prompt=""" 
            When did the State of the Union Address take place?
         """,
         agent=KnowledgeManagementAgentMetadata(
            name="sotu",
            type="generic-agent",
            description="Knowledge Management Agent that queries the State of the Union speech transcript.",
            language_model=LanguageModel(
                type=LanguageModelType.OPENAI,
                provider=LanguageModelProvider.MICROSOFT,
                temperature=0,
                use_chat=True
            ),
            indexing_profile_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/indexingprofiles/sotu-index",
            text_embedding_profile_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Vectorization/textembeddingprofiles/AzureOpenAI_Embedding",
            prompt_object_id="/instances/11111111-1111-1111-1111-111111111111/providers/FoundationaLLM.Prompt/prompts/sotu",
            sessions_enabled=True,
            conversation_history = ConversationHistory(enabled=True, max_history=5),
            gatekeeper=Gatekeeper(use_system_setting=True, options=["ContentSafety", "Presidio"])
         ),
         message_history = [
            {
                "sender": "User",
                "text": "What is your name?"
            },
            {
                "sender": "Assistant",
                "text": "My name is Baldwin."
            }
        ]
     )
     return req

class OrchestrationManagerTests:
    def test_orchestration_manager_initializes(self, test_azure_ai_search_service_completion_request, test_config, test_context, test_resource_provider):
        manager = OrchestrationManager(completion_request=test_azure_ai_search_service_completion_request, configuration=test_config, context=test_context, resource_provider=test_resource_provider)
        assert manager is not None
        
    def test_orchestration_manager_resolves_correct_agent(self, test_azure_ai_search_service_completion_request, test_config, test_context, test_resource_provider):
        manager = OrchestrationManager(completion_request=test_azure_ai_search_service_completion_request, configuration=test_config, context=test_context, resource_provider=test_resource_provider)
        agent = manager.agent        
        assert type(agent) == KnowledgeManagementAgent
        
    def test_orchestration_manager_runs_correct_agent(self, test_azure_ai_search_service_completion_request, test_config, test_context, test_resource_provider):
        manager = OrchestrationManager(completion_request=test_azure_ai_search_service_completion_request, configuration=test_config, context=test_context, resource_provider=test_resource_provider)
        response = manager.run(test_azure_ai_search_service_completion_request.user_prompt)       
        assert "february" in response.completion.lower()