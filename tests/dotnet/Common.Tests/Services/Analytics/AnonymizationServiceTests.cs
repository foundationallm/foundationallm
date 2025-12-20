using FoundationaLLM.Common.Models.Configuration.Analytics;
using FoundationaLLM.Common.Services.Analytics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoundationaLLM.Common.Tests.Services.Analytics
{
    public class AnonymizationServiceTests
    {
        private readonly ILogger<AnonymizationService> _logger;
        private readonly AnonymizationService _service;
        private readonly AnalyticsSettings analyticsSettings = new();

        public AnonymizationServiceTests()
        {
            _logger = Substitute.For<ILogger<AnonymizationService>>();
            _service = new AnonymizationService(
                Options.Create<AnalyticsSettings>(analyticsSettings),
                _logger);
        }

        [Fact]
        public void AnonymizeUPN_WithNull_ReturnsAnonymous()
        {
            var result = _service.AnonymizeUPN(null);
            Assert.Equal("anonymous", result);
        }

        [Fact]
        public void AnonymizeUPN_WithEmpty_ReturnsAnonymous()
        {
            var result = _service.AnonymizeUPN("");
            Assert.Equal("anonymous", result);
        }

        [Fact]
        public void AnonymizeUPN_WithNA_ReturnsAnonymous()
        {
            var result = _service.AnonymizeUPN("N/A");
            Assert.Equal("anonymous", result);
        }

        [Fact]
        public void AnonymizeUPN_WithValidUPN_ReturnsHashedValue()
        {
            var result = _service.AnonymizeUPN("user@example.com");
            Assert.NotEqual("anonymous", result);
            Assert.NotEqual("user@example.com", result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public void AnonymizeUserId_WithNull_ReturnsAnonymous()
        {
            var result = _service.AnonymizeUserId(null);
            Assert.Equal("anonymous", result);
        }

        [Fact]
        public void AnonymizeUserId_WithValidId_ReturnsHashedValue()
        {
            var result = _service.AnonymizeUserId("user123");
            Assert.NotEqual("anonymous", result);
            Assert.NotEqual("user123", result);
        }
    }
}
