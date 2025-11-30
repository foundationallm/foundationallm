using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Analytics;
using FoundationaLLM.Common.Models.Configuration.Analytics;
using FoundationaLLM.Common.Services.Analytics;
using FoundationaLLM.Common.Services.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace FoundationaLLM.Common.Tests.Services.Analytics
{
    public class AbuseDetectionServiceTests
    {
        private readonly IAzureCosmosDBService _cosmosDBService;
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AbuseDetectionService> _logger;
        private readonly AbuseDetectionService _service;
        private readonly AnalyticsSettings analyticsSettings = new();

        public AbuseDetectionServiceTests()
        {
            _cosmosDBService = Substitute.For<IAzureCosmosDBService>();
            _analyticsService = Substitute.For<IAnalyticsService>();
            _logger = Substitute.For<ILogger<AbuseDetectionService>>();
            _service = new AbuseDetectionService(
                _analyticsService,
                Options.Create<AnalyticsSettings>(analyticsSettings),
                _logger);
        }

        [Fact]
        public async Task CalculateAbuseRiskScoreAsync_ReturnsScore()
        {
            // Arrange
            var instanceId = "test-instance";
            var username = "user@example.com";
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            _analyticsService
                .GetUserAnalyticsSummaryAsync(instanceId, username, startDate, endDate, Arg.Any<CancellationToken>())
                .Returns(new UserAnalyticsSummary
                {
                    Username = username,
                    TotalRequests = 100,
                    TotalTokens = 50000,
                    ErrorRate = 5.0
                });

            _analyticsService
                .GetUserActivityTimelineAsync(instanceId, username, startDate, endDate, Arg.Any<CancellationToken>())
                .Returns(new UserActivityTimeline
                {
                    Username = username,
                    Entries = new List<UserActivityEntry>()
                });

            // Act
            var result = await _service.CalculateAbuseRiskScoreAsync(instanceId, username, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.InRange(result.Score, 0, 100);
            Assert.NotNull(result.Factors);
        }

        [Fact]
        public async Task DetectAbuseIndicatorsAsync_ReturnsIndicators()
        {
            // Arrange
            var instanceId = "test-instance";
            var username = "user@example.com";
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            _analyticsService
                .GetUserAnalyticsSummaryAsync(instanceId, username, startDate, endDate, Arg.Any<CancellationToken>())
                .Returns(new UserAnalyticsSummary
                {
                    Username = username,
                    TotalRequests = 1000,
                    TotalTokens = 500000,
                    ErrorRate = 10.0
                });

            _analyticsService
                .GetUserActivityTimelineAsync(instanceId, username, startDate, endDate, Arg.Any<CancellationToken>())
                .Returns(new UserActivityTimeline
                {
                    Username = username,
                    Entries = new List<UserActivityEntry>()
                });

            // Act
            var result = await _service.DetectAbuseIndicatorsAsync(instanceId, username, startDate, endDate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.NotNull(result.VolumeIndicators);
            Assert.NotNull(result.TemporalIndicators);
            Assert.NotNull(result.BehavioralIndicators);
            Assert.NotNull(result.ResourceIndicators);
        }
    }
}
