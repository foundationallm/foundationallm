using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceReferenceTests
    {
        [Fact]
        public void ResourceReference_SetValues_CheckProperties()
        {
            // Arrange
            string expectedName = "ResourceName";
            string expectedFilename = "ResourceFileName";
            string expectedType = "FileType";

            // Act
            var resourceReference = new ResourceReference
            {
                Name = expectedName,
                Filename = expectedFilename,
                Type = expectedType
            };

            // Assert
            Assert.Equal(expectedName, resourceReference.Name);
            Assert.Equal(expectedFilename, resourceReference.Filename);
            Assert.Equal(expectedType, resourceReference.Type);
        }
    }
}
