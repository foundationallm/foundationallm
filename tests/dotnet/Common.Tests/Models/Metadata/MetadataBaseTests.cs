using FoundationaLLM.Common.Models.Metadata;

namespace FoundationaLLM.Common.Tests.Models.Metadata
{
    public class MetadataBaseTests
    {
        [Fact]
        public void MetadataBase_Properties_Initialized_Correctly()
        {
            // Arrange
            var metadata = new MetadataBase();

            // Assert
            Assert.Null(metadata.Name);
            Assert.Null(metadata.Type);
            Assert.Null(metadata.Description);
        }
    }
}
