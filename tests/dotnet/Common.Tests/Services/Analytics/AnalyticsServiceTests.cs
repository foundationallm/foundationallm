using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Analytics;
using FoundationaLLM.Common.Services.Analytics;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace FoundationaLLM.Common.Tests.Services.Analytics
{
    public class AnalyticsServiceTests
    {
        private readonly IAzureCosmosDBService _cosmosDBService;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly AnalyticsService _service;

        public AnalyticsServiceTests()
        {
            _cosmosDBService = Substitute.For<IAzureCosmosDBService>();
            _logger = Substitute.For<ILogger<AnalyticsService>>();
            _service = new AnalyticsService(
                _cosmosDBService,
                _logger);
        }

        [Fact]
        public async Task GetAnalyticsOverviewAsync_ReturnsOverview()
        {
            // Arrange
            var instanceId = "test-instance";
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            // Mock count query result
            var countResult = new List<JsonElement>
            {
                JsonSerializer.Deserialize<JsonElement>("10")
            };
            _cosmosDBService
                .QueryItemsAsync<JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    Arg.Any<Microsoft.Azure.Cosmos.QueryDefinition>(),
                    Arg.Any<CancellationToken>())
                .Returns(countResult);

            // Act
            var result = await _service.GetAnalyticsOverviewAsync(instanceId, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AnalyticsOverview>(result);
        }

        [Fact]
        public async Task GetTopUsersAsync_ReturnsUsers()
        {
            // Arrange
            var instanceId = "test-instance";
            var topCount = 10;
            var sortBy = UserSortBy.Requests;
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            var mockResults = new List<JsonElement>
            {
                JsonSerializer.Deserialize<JsonElement>(@"{
                    ""username"": ""user1@example.com"",
                    ""totalSessions"": 5,
                    ""totalTokens"": 1000,
                    ""lastActivity"": 1234567890
                }")
            };

            _cosmosDBService
                .QueryItemsAsync<JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    Arg.Any<Microsoft.Azure.Cosmos.QueryDefinition>(),
                    Arg.Any<CancellationToken>())
                .Returns(mockResults);

            // Act
            var result = await _service.GetTopUsersAsync(instanceId, topCount, sortBy, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<TopUserSummary>>(result);
        }

        [Fact]
        public async Task GetUserAnalyticsSummaryAsync_ReturnsSummary()
        {
            // Arrange
            var instanceId = "test-instance";
            var username = "user@example.com";
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;

            var mockResults = new List<JsonElement>
            {
                JsonSerializer.Deserialize<JsonElement>(@"{
                    ""sessionCount"": 5,
                    ""totalTokens"": 1000,
                    ""avgTokens"": 200
                }")
            };

            _cosmosDBService
                .QueryItemsAsync<JsonElement>(
                    AzureCosmosDBContainers.Sessions,
                    Arg.Any<Microsoft.Azure.Cosmos.QueryDefinition>(),
                    Arg.Any<CancellationToken>())
                .Returns(mockResults);

            // Act
            var result = await _service.GetUserAnalyticsSummaryAsync(instanceId, username, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
        }
    }
}
