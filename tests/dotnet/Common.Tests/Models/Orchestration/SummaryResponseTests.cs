using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class SummaryResponseTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedInfo = "Info_1";

            // Act
            var summaryResponse = new SummaryResponse
            {
                Summary = expectedInfo
            };

            // Assert
            Assert.Equal(expectedInfo, summaryResponse.Summary);
        }

    }
}
