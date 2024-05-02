using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceNameTests
    {
        [Fact]
        public void ResourceName_Properties_Test()
        {
            // Arrange
            string expectedName = "Test resource";
            string expectedType = "Test type";

            // Act
            var resourceName = new ResourceName
            {
                Name = expectedName,
                Type = expectedType
            };

            // Assert
            Assert.Equal(expectedName, resourceName.Name);
            Assert.Equal(expectedType, resourceName.Type);
        }
    }
}
