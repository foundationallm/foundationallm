using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceBaseTests
    {
        [Fact]
        public void ResourceBase_Properties_Test()
        {
            // Arrange
            string expectedObjectId = "12345";
            string expectedDescription = "Test description";
            string expectedName = "Test resource";
            string expectedType = "Test type";

            // Act
            var resource = new ResourceBase
            {
                ObjectId = expectedObjectId,
                Description = expectedDescription,
                Name = expectedName,
                Type = expectedType
            };

            // Assert
            Assert.Equal(expectedObjectId, resource.ObjectId);
            Assert.Equal(expectedDescription, resource.Description);
            Assert.Equal(expectedName, resource.Name);
            Assert.Equal(expectedType, resource.Type);
        }
    }
}
