using FoundationaLLM.Client.Core;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.ClientModel;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.SDK
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_SDK_CoreAPI(
        ITestOutputHelper output,
        TestFixture fixture) : TestBase(1, output, fixture, new DependencyInjectionContainerInitializer()), IClassFixture<TestFixture>
    {
        private CoreClient _coreClient = null!;
        private IConfiguration _configuration = null!;

        [Fact]
        public async Task SDK_CoreAPI_Create_Conversation_And_Completion()
        {
            InitializeCoreClient();

            WriteLine("============ FoundationaLLM SDK - Core API Client Tests ============");

            var conversationId = await _coreClient.CreateChatSessionAsync(
                new ConversationProperties
                {
                    Name = "Test Conversation"
                });

            Assert.True(!string.IsNullOrEmpty(conversationId));

            var completionResponse = await _coreClient.GetCompletionWithSessionAsync(
                conversationId,
                new ConversationProperties
                {
                    Metadata = $"{{\"ProjectId\": {_configuration["CoreClient:ProjectId"]!}}}"
                },
                _configuration["CoreClient:UserPrompt"]!,
                _configuration["CoreClient:AgentName"]!);

            Assert.True(!string.IsNullOrEmpty(completionResponse.Content?.FirstOrDefault()?.Value));
        }

        private void InitializeCoreClient()
        {
            _configuration = GetService<IConfiguration>();

            _coreClient = new CoreClient(
                _configuration["CoreClient:CoreAPIUrl"]!,
                new ApiKeyCredential(
                    _configuration["FoundationaLLM:Tests:AgentAccessToken"]!),
                _configuration["CoreClient:InstanceId"]!);
        }
    }
}