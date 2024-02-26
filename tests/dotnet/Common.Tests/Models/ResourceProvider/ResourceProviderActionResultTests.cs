using FoundationaLLM.Common.Models.ResourceProvider;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceProviderActionResultTests
    {
        [Fact]
        public void ResourceProviderActionResult_IsSuccessResult_True_Test()
        {
            // Arrange
            bool isSuccess = true;

            // Act
            var result = new ResourceProviderActionResult(isSuccess);

            // Assert
            Assert.True(result.IsSuccessResult);
        }
    }
}
