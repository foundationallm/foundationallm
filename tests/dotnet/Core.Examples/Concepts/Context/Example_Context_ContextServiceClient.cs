using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Context
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_Context_ContextServiceClient(
        ITestOutputHelper output,
        TestFixture fixture) : TestBase(1, output, fixture, new DependencyInjectionContainerInitializer()), IClassFixture<TestFixture>
    {
        private IContextServiceClient _contextServiceClient = null!;
        private IConfiguration _configuration = null!;

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task Context_ContextServiceClient_GetFileContent(
            string instanceId,
            string fileId)
        {
            InitializeContextServiceClient();

            WriteLine("============ FoundationaLLM Context - Context Service Client Tests ============");

            var response = await _contextServiceClient.GetFileContent(
                instanceId,
                fileId);

            Assert.True(response.Success);
        }

        private void InitializeContextServiceClient()
        {
            _configuration = GetService<IConfiguration>();

            _contextServiceClient = new ContextServiceClient(
                new OrchestrationContext { CurrentUserIdentity = ServiceContext.ServiceIdentity },
                GetService<IHttpClientFactoryService>(),
                GetService<ILogger<ContextServiceClient>>());
        }

        public static TheoryData<string, string> TestData =>
        new()
        {
            {
                "8ac6074c-bdde-43cb-a140-ec0002d96d2b",
                "file-20251001-100848-av4Y-DT-sUOMVvza1EW5Zg"
            }
        };
    }
}