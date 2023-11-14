using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Tests.Services
{
    public class DataHubAPIServiceTests
    {
        private readonly IOptions<DataSourceHubSettings> _options = Substitute.For<IOptions<DataSourceHubSettings>>();
        private readonly ILogger<DataSourceHubAPIService> _logger = Substitute.For<ILogger<DataSourceHubAPIService>>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly DataSourceHubAPIService _dataSourceHubAPIService;

        public DataHubAPIServiceTests()
        {
            _dataSourceHubAPIService = new DataSourceHubAPIService(_options, _logger, _httpClientFactoryService);
        }

        [Fact]
        public async Task Status_Success_ReturnsStatus()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                Content = new StringContent("TestStatus", Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI).Returns(httpClient);

            // Act
            var result = await _dataSourceHubAPIService.Status();

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestStatus", result);
        }

        [Fact]
        public async Task ResolveRequest_Success_ReturnsDataSourceHubResponse()
        {
            // Arrange
            var sources = new List<string> { "Source1", "Source2" };

            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new DataSourceHubResponse
                {
                    DataSources = new List<DataSourceMetadata> { new DataSourceMetadata { Name = "DataSource1"} }
                }), Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.OK
            };

            var httpClient = new HttpClient(new FakeMessageHandler(response))
            {
                BaseAddress = new Uri("http://nsubstitute.io")
            };
            _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI).Returns(httpClient);

            // Act
            var result = await _dataSourceHubAPIService.ResolveRequest(sources);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.DataSources?.Select(x=>x.Name));
        }
    }
}
