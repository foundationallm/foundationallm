using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceNameCheckResultTests
    {
        [Fact]
        public void ResourceNameCheckResult_Properties_Test()
        {
            // Arrange
            string expectedName = "TestResource";
            string expectedType = "TestType";
            NameCheckResultType expectedStatus = NameCheckResultType.Allowed;
            string expectedMessage = "TestMessage";

            // Act
            var resourceNameCheckResult = new ResourceNameCheckResult(
                Name: expectedName,
                Type: expectedType,
                Status: expectedStatus,
                Exists: false,
                Deleted: false,
                ErrorMessage: expectedMessage);

            // Assert
            Assert.Equal(expectedName, resourceNameCheckResult.Name);
            Assert.Equal(expectedType, resourceNameCheckResult.Type);
            Assert.Equal(expectedStatus, resourceNameCheckResult.Status);
            Assert.Equal(expectedMessage, resourceNameCheckResult.ErrorMessage);
        }
    }
}
