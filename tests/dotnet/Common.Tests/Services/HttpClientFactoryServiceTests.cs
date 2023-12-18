using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Services;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Models.Metadata;
using NSubstitute.Core;

namespace FoundationaLLM.Common.Tests.Services
{
    public class HttpClientFactoryServiceTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICallContext _callContext;
        private readonly IDownstreamAPISettings _apiSettings;

        public HttpClientFactoryServiceTests()
        {
            _httpClientFactory = Substitute.For<IHttpClientFactory>();
            _callContext = Substitute.For<ICallContext>();
            _apiSettings = Substitute.For<IDownstreamAPISettings>();
        }

        [Fact]
        public void CreateClient_WithValidClientName_ReturnsHttpClientWithHeaders()
        {
            // Arrange
            var clientName = "TestClient";
            var apiKey = "TestAPIKey";
            var userContext = new UnifiedUserIdentity
            {
                UPN = "TestUPN",
                Username = "TestUsername",
                Name = "TestName"
            };
            var agentHint = new Agent
            {
                Name = "TestAgentHint",
                Private = false
            };

            _apiSettings.DownstreamAPIs.Returns(new System.Collections.Generic.Dictionary<string, DownstreamAPIKeySettings>
            {
                { 
                    clientName, new DownstreamAPIKeySettings
                    {
                        APIUrl = "TestAPIUrl",
                        APIKey = "TestAPIKey"
                    }
                }
            });

            _callContext.CurrentUserIdentity.Returns(userContext);
            _callContext.AgentHint.Returns(agentHint);

            var httpClient = new HttpClient();
            _httpClientFactory.CreateClient(clientName).Returns(httpClient);

            var service = new HttpClientFactoryService(_httpClientFactory, _callContext, _apiSettings);

            // Act
            var result = service.CreateClient(clientName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(httpClient.Timeout, TimeSpan.FromSeconds(600));
            Assert.Equal(result.DefaultRequestHeaders.GetValues(Constants.HttpHeaders.APIKey), new[] { apiKey });
            Assert.Equal(result.DefaultRequestHeaders.GetValues(Constants.HttpHeaders.UserIdentity), new[] { JsonConvert.SerializeObject(userContext) });
            Assert.Equal(result.DefaultRequestHeaders.GetValues(Constants.HttpHeaders.AgentHint), new[] { JsonConvert.SerializeObject(agentHint) });
        }
    }
}
