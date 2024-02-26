using FoundationaLLM.Common.Models.ResourceProvider;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceProviderUpsertResultTests
    {
        [Fact]
        public void ResourceProviderUpsertResult_ObjectId_Set_Test()
        {
            // Arrange
            string expectedObjectId = "12345";

            // Act
            var result = new ResourceProviderUpsertResult
            {
                ObjectId = expectedObjectId
            };

            // Assert
            Assert.Equal(expectedObjectId, result.ObjectId);
        }
    }
}
