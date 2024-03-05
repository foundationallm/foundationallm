import pytest
from foundationallm.config import Configuration, Context, UserIdentity
from foundationallm.models.metadata import KnowledgeManagementAgent as KnowledgeManagementAgentMetadata
from foundationallm.models.metadata import ConversationHistory, Gatekeeper
from foundationallm.models.language_models import LanguageModelType, LanguageModelProvider, LanguageModel
from foundationallm.models.orchestration import KnowledgeManagementCompletionRequest
from foundationallm.langchain.agents import KnowledgeManagementAgent
from foundationallm.langchain.orchestration import OrchestrationManager
from foundationallm.resources import ResourceProvider

@pytest.fixture
def test_config():
    return Configuration()

@pytest.fixture
def test_resource_provider(test_config):
    return ResourceProvider(config=test_config)

@pytest.fixture
def test_context():
    return Context(user_identity='{"name": "test", "user_name": "test@foundationallm.ai" , "upn": "test@foundationallm.ai"}')

@pytest.fixture
def test_internal_context_completion_request():
     req = KnowledgeManagementCompletionRequest(
         user_prompt=""" 
         You are an expert an very specific Trivia. Your knowledgebase consists of the following facts:
         1. Colorful Currency: The residents of Canada have a penchant for humor when it comes to their currency. Canadian dollars are sometimes affectionately referred to as loonies and toonies, named after the common loon bird depicted on the one-dollar coin. The toonie, on the other hand, features a polar bear on the reverse side and gets its name from combining two with loonie, even though it does not depict a loon. The term toonie has become widely accepted slang for the two-dollar coin. This playful nomenclature showcases the Canadian ability to infuse a bit of fun into everyday financial transactions.
         2. The Law of the Letter: In 1928, a Michigan senator proposed an odd law to bolster local business: a bill to put a crew of phonetic experts together to simplify the spelling of 300 common English words. The goal was to encourage literacy and save on printing costs by reducing the number of letters. While it didn't pass, the notion that the state might mandate "kiss" to be spelled "kis" is a charming example of legislative quirkiness.
         3. The Great Emu War: In 1932, Australia faced an unexpected "foe" when emus, large flightless birds, began invading farmland in Western Australia. The military was called in to manage the birds with machine guns in what became known as the "Great Emu War." However, despite their efforts, the emus proved elusive and difficult to combat, leading to a retreat by the armed forces and a victory for the birds, which has been a source of amusement and national lore ever since.
         4. A Nobel Prank: When Andre Geim won the Nobel Prize in Physics in 2010, he became the first, and so far only, person to have received both a Nobel Prize and an Ig Nobel Prize, which is awarded for unusual or trivial achievements in scientific research. His Nobel was for groundbreaking work on graphene, while his Ig Nobel celebrated his earlier experiment of levitating a frog with magnets. This demonstrates that even the most distinguished scientists have a lighter side.
         5. Astronaut Shenanigans: In 1965, astronaut John Young smuggled a corned beef sandwich into space aboard Gemini 3, hiding it in a pocket of his spacesuit. This unauthorized snack led to a minor congressional hearing, as crumbs in zero gravity could have been a hazard. The incident prompted stricter controls on what could be brought aboard spacecraft, but it also left a legacy of one of the most unusual and delicious items ever to reach orbit.
         -------------------
         Answer only questions related to the facts you know, if you don't know the answer, just type: I don't know.
           
         Question: What is the nickname for two Canadian dollars?
         """,
         agent=KnowledgeManagementAgentMetadata(
            name="internal-context",
            type="knowledge-management",
            description="Session-less agent that issues the user prompt directly to the language model.",
            language_model=LanguageModel(
                type=LanguageModelType.OPENAI,
                provider=LanguageModelProvider.MICROSOFT,
                temperature=0,
                use_chat=True
            )            
         )
     )
     return req

@pytest.fixture
def test_azure_ai_search_service_completion_request():
     req = KnowledgeManagementCompletionRequest(
         user_prompt=""" 
            When did the State of the Union Address take place?
         """,
         agent=KnowledgeManagementAgentMetadata(
            name="sotu",
            type="knowledge-management",
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