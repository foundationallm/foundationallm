using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running the default FoundationaLLM agent completions in both session and sessionless modes.
    /// </summary>
    public class Example0001_FoundationaLLMAgentInteraction : BaseTest, IClassFixture<TestFixture>
	{
		private readonly IAgentConversationTestService _agentConversationTestService;

		public Example0001_FoundationaLLMAgentInteraction(ITestOutputHelper output, TestFixture fixture)
			: base(output, [fixture.ServiceProvider])
		{
            _agentConversationTestService = GetService<IAgentConversationTestService>();
		}

		[Fact]
		public async Task RunAsync()
		{
			WriteLine("============ FoundationaLLM Agent Completions ============");
			await RunExampleAsync();
		}

		private async Task RunExampleAsync()
        {
            var userPrompt = "Who are you?";
            var agentName = Constants.TestAgentNames.FoundationaLLMAgentName;

            WriteLine($"Send session-based \"{userPrompt}\" user prompt to the {agentName} agent.");
            var response = await _agentConversationTestService.RunAgentCompletionWithSession(agentName, userPrompt, null, false);
            WriteLine($"Agent completion response: {response.Text}");
            Assert.False(string.IsNullOrWhiteSpace(response.Text) || response.Text == Constants.TestResponseMessages.FailedCompletionResponse);
            WriteLine($"Send sessionless \"{userPrompt}\" user prompt to the {agentName} agent.");
            response = await _agentConversationTestService.RunAgentCompletionWithNoSession(agentName, userPrompt, false);
            WriteLine($"Agent completion response: {response.Text}");
            Assert.False(string.IsNullOrWhiteSpace(response.Text) || response.Text == Constants.TestResponseMessages.FailedCompletionResponse);
        }
	}
}
