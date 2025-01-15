using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Services.Templates;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FoundationaLLM.Core.Tests.Services
{
    public class RegexTemplatingServiceTests
    {
        private readonly ILogger<RegexTemplatingService> _logger = Substitute.For<ILogger<RegexTemplatingService>>();

        [Fact]
        public async Task ReplaceCurrentDateTimeVariable()
        {
            // Arrange

            ITemplatingService templatingService = new RegexTemplatingService(_logger);

            // Act
            var inputString = "The current date is {{foundationallm:current_datetime_utc:dddd, MMMM dd, yyyy}}. This looks great.";
            var outputString = templatingService.Transform(inputString);
            var expectedOutputString = $"The current date is {DateTime.UtcNow:dddd, MMMM dd, yyyy}. This looks great.";


            // Assert
            Assert.Equal(expectedOutputString, outputString);
        }
    }
}
