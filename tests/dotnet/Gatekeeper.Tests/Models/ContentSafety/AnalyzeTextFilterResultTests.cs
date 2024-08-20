using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;

namespace Gatekeeper.Tests.Models.ContentSafety
{
    public class AnalyzeTextFilterResultTests
    {
        [Fact]
        public void AnalyzeTextFilterResult_Properties_SetCorrectly()
        {
            // Arrange
            var analyzeTextFilterResult = new AnalyzeTextFilterResult
            {
                Safe = true,
                Reason = "Reason_1"
            };

            // Assert
            Assert.True(analyzeTextFilterResult.Safe);
            Assert.Equal("Reason_1", analyzeTextFilterResult.Reason);
        }
    }
}
