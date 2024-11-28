using FoundationaLLM.Core.Examples.Constants;
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
            var responseText = response.Content!.First().Value;
            WriteLine($"Agent completion response: {responseText}");
            Assert.False(string.IsNullOrWhiteSpace(responseText) || string.Equals(responseText, TestResponseMessages.FailedCompletionResponse));
        }
	}
}
