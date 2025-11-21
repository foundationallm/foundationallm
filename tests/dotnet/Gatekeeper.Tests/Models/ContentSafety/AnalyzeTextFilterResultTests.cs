using FoundationaLLM.Common.Models.ContentSafety;

namespace Gatekeeper.Tests.Models.ContentSafety
{
    public class AnalyzeTextFilterResultTests
    {
        [Fact]
        public void AnalyzeTextFilterResult_Properties_SetCorrectly()
        {
            // Arrange
            var analyzeTextFilterResult = new ContentSafetyAnalysisResult
            {
                SafeContent = true,
                Details = "Reason_1"
            };

            // Assert
            Assert.True(analyzeTextFilterResult.SafeContent);
            Assert.Equal("Reason_1", analyzeTextFilterResult.Details);
        }
    }
}
