﻿using FoundationaLLM.Core.Examples.Constants;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for sending user queries to a Knowledge Management with inline context agent using the SemanticKernel orchestrator.
    /// </summary>
    public class Example0002_KnowledgeManagementInlineContextAgentWithSemanticKernel : TestBase, IClassFixture<TestFixture>
    {
        private readonly IAgentConversationTestService _agentConversationTestService;
        private readonly IManagementAPITestManager _managementAPITestManager;

        public Example0002_KnowledgeManagementInlineContextAgentWithSemanticKernel(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture)
        {
            _agentConversationTestService = GetService<IAgentConversationTestService>();
            _managementAPITestManager = GetService<IManagementAPITestManager>();
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Knowledge Management with inline context agent using SemanticKernel ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            var agentName = Constants.TestAgentNames.SemanticKernelInlineContextAgentName;
            try
            {
                var userPrompts = new List<string>
                {
                    "Who are you?",
                    "What is the significance of the Rosetta Stone in the history of linguistics?",
                    "What was the Rosetta Stone's role in ancient political dynamics?",
                    "How did the decipherment of the Rosetta Stone impact the study of ancient Egypt?"
                };

                WriteLine($"Send Rosetta Stone questions to the {agentName} agent.");

                var response = await _agentConversationTestService.RunAgentConversationWithSession(
                    agentName, userPrompts, null, true);

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
                Assert.True(invalidAgentResponsesFound == 0, $"{invalidAgentResponsesFound} invalid agent responses found.");
            }
            catch (Exception ex)
            {
                WriteLine($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                await _managementAPITestManager.DeleteAgent(agentName);
            }
        }
    }
}