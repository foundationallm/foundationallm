using FoundationaLLM.Core.Examples.Constants;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for the Knowledge Management agent with SemanticKernel.
    /// </summary>
    public class Example0012_KnowledgeManagementAgentWithLangChain : BaseTest, IClassFixture<TestFixture>
    {
        private readonly IAgentConversationTestService _agentConversationTestService;
        private readonly IVectorizationTestService _vectorizationTestService;

        private string textEmbeddingProfileName = "text_embedding_profile_generic";
        private string indexingProfileName = "indexing_profile_sdzwa";

        public Example0012_KnowledgeManagementAgentWithLangChain(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture.ServiceProvider)
        {
            _agentConversationTestService = GetService<IAgentConversationTestService>();
            _vectorizationTestService = GetService<IVectorizationTestService>();
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Knowledge Management agent with Lang Chain on SDZWA ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            var agentName = TestAgentNames.LangChainSDZWA;
            var userPrompts = new List<string>
            {
                "Who are you?",
                "Tell me one interesting facts about the San Diego Zoo?",
                "How many animals does the San Diego Zoo host?",
                "What does the San Diego Zoo do to treat illness among it's inhabitants?"
            };

            WriteLine($"Send questions to the {agentName} agent.");

            await _vectorizationTestService.CreateIndexingProfile(indexingProfileName);
            await _vectorizationTestService.CreateTextEmbeddingProfile(textEmbeddingProfileName);

            var response = await _agentConversationTestService.RunAgentConversationWithSession(
                agentName, userPrompts, null, true, indexingProfileName, textEmbeddingProfileName);

            WriteLine($"Agent conversation history:");

            var invalidAgentResponsesFound = 0;
            foreach (var message in response)
            {
                WriteLine($"- {message.Sender}: {message.Text}");

                if (string.Equals(message.Sender, Common.Constants.Agents.InputMessageRoles.Assistant, StringComparison.CurrentCultureIgnoreCase) &&
                    message.Text == TestResponseMessages.FailedCompletionResponse)
                {
                    invalidAgentResponsesFound++;
                }
            }

            await _vectorizationTestService.DeleteIndexingProfile(indexingProfileName, false);
            await _vectorizationTestService.DeleteTextEmbeddingProfile(textEmbeddingProfileName);

            Assert.True(invalidAgentResponsesFound == 0, $"{invalidAgentResponsesFound} invalid agent responses found.");
        }
    }
}
