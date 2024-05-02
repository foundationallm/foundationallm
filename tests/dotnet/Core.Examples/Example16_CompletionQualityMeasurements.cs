using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.AzureAIService;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Core.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running agent completions and evaluating the quality of the completions using Azure AI Studio.
    /// </summary>
    public class Example16_CompletionQualityMeasurements : BaseTest, IClassFixture<TestFixture>
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ICosmosDbService _cosmosDbService;
		private readonly IAzureAIService _azureAIService;
		private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

		public Example16_CompletionQualityMeasurements(ITestOutputHelper output, TestFixture fixture)
			: base(output, fixture.ServiceProvider)
		{
			_httpClientFactory = GetService<IHttpClientFactory>();
			_cosmosDbService = GetService<ICosmosDbService>();
			_azureAIService = GetService<IAzureAIService>();
		}

		[Fact]
		public async Task RunAsync()
		{
			WriteLine("============ Agent Completions ============");
			await RunExampleAsync();
		}

		private async Task RunExampleAsync()
		{
			var agentPrompts = TestConfiguration.CompletionQualityMeasurementConfiguration.AgentPrompts;
			if (agentPrompts == null || agentPrompts.Length == 0)
			{
				WriteLine("No agent prompts found. Make sure you enter them in testsettings.json.");
				return;
			}
			foreach (var agentPrompt in agentPrompts)
			{
				await RunAgentCompletionAsync(agentPrompt);
			}
		}

		private async Task RunAgentCompletionAsync(AgentPrompt agentPrompt)
		{
			WriteLine($"Agent: {agentPrompt.AgentName}");

			var client = await GetHttpClient();
			if (agentPrompt.SessionConfiguration is {CreateNewSession: true})
			{
				var responseSession = await client.PostAsync("sessions", null);

				if (responseSession.IsSuccessStatusCode)
				{
					var responseContent = await responseSession.Content.ReadAsStringAsync();
					var sessionResponse = JsonSerializer.Deserialize<Session>(responseContent, _jsonSerializerOptions);
					if (sessionResponse?.SessionId != null)
					{
						agentPrompt.SessionConfiguration.SessionId = sessionResponse.SessionId;
					}

					var sessionName = "Test: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
					var responseUpdate = await client.PostAsync($"sessions/{agentPrompt.SessionConfiguration.SessionId}/rename?newChatSessionName={UrlEncoder.Default.Encode(sessionName)}", null);

					if (!responseUpdate.IsSuccessStatusCode)
					{
						WriteLine("Failed to update the session name.");
					}
				}
			}
			
			if (agentPrompt.SessionConfiguration != null)
			{
				var orchestrationRequest = new OrchestrationRequest
				{
					SessionId = agentPrompt.SessionConfiguration.SessionId,
					AgentName = agentPrompt.AgentName,
					UserPrompt = agentPrompt.UserPrompt ?? string.Empty,
					Settings = null
				};

				var serializedRequest = JsonSerializer.Serialize(orchestrationRequest, _jsonSerializerOptions);

				var sessionUrl = $"sessions/{agentPrompt.SessionConfiguration.SessionId}/completion"; // Session-based - message history and data is retained in Cosmos DB. Must create a session if it does not exist.
				var responseMessage = await client.PostAsync(sessionUrl,
					new StringContent(
						serializedRequest,
						Encoding.UTF8, "application/json"));

				if (responseMessage.IsSuccessStatusCode)
				{
					var responseContent = await responseMessage.Content.ReadAsStringAsync();
					var completionResponse = JsonSerializer.Deserialize<Completion>(responseContent, _jsonSerializerOptions);

					var session = await _cosmosDbService.GetSessionAsync(agentPrompt.SessionConfiguration.SessionId!);
					var messages = await _cosmosDbService.GetSessionMessagesAsync(session.SessionId, session.UPN);
					// Get the last message where the agent is the sender.
					var lastAgentMessage = messages.LastOrDefault(m => m.Sender == nameof(Participants.Assistant));
					if (lastAgentMessage != null && !string.IsNullOrWhiteSpace(lastAgentMessage.CompletionPromptId))
					{
						// Get the completion prompt from the last agent message.
						var completionPrompt = await _cosmosDbService.GetCompletionPrompt(session.SessionId, lastAgentMessage.CompletionPromptId);
						// For the context, take everything in the prompt that comes after `\\n\\nContext:\\n`. If it doesn't exist, take the whole prompt.
						var contextIndex = completionPrompt.Prompt.IndexOf(@"\n\nContext:\n", StringComparison.Ordinal);
						if (contextIndex != -1)
						{
							completionPrompt.Prompt = completionPrompt.Prompt[(contextIndex + 14)..];
						}
						var dataSet = new InputsMapping
						{
							Question = agentPrompt.UserPrompt,
							Answer = completionResponse?.Text,
							Context = completionPrompt.Prompt,
							GroundTruth = agentPrompt.ExpectedCompletion,
						};
						// Create a new Azure AI evaluation from the data.
						var dataSetName = $"{agentPrompt.AgentName}_{session.SessionId}";
						var dataSetPath = await _azureAIService.CreateDataSet(dataSet, dataSetName);
						var dataSetVersion = await _azureAIService.CreateDataSetVersion(dataSetName, dataSetPath);
						_ = int.TryParse(dataSetVersion.DataVersion.VersionId, out var dataSetVersionNumber);
						var jobId = await _azureAIService.SubmitJob(dataSetName, dataSetName, dataSetVersionNumber == 0 ? 1 : dataSetVersionNumber,
							string.Empty);
						WriteLine($"Azure AI evaluation Job ID -> {jobId}");
					}

					WriteLine($"User prompt -> '{agentPrompt.UserPrompt}'");
					WriteLine($"Agent completion -> '{completionResponse?.Text}'");
					WriteLine($"Expected completion -> '{agentPrompt.ExpectedCompletion}'");
					WriteLine("-------------------------------");
				}
			}
		}

		private async Task<HttpClient> GetHttpClient()
		{
			var httpClient = _httpClientFactory.CreateClient(HttpClients.CoreAPI);
			var coreApiScope = TestConfiguration.GetAppConfigValueAsync(AppConfigurationKeys.FoundationaLLM_Chat_Entra_Scopes).GetAwaiter().GetResult();

			// The scope needs to just be the base URI, not the full URI.
			coreApiScope = coreApiScope[..coreApiScope.LastIndexOf('/')];

			var credentials = TestConfiguration.GetTokenCredential();
			var tokenResult = await credentials.GetTokenAsync(
				new([coreApiScope]),
				default);

			httpClient.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Bearer", tokenResult.Token);

			return httpClient;
		}
	}
}
