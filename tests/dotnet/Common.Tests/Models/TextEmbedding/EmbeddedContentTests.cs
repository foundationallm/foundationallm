using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Tests.Models.TextEmbedding
{
    public class EmbeddedContentTests
    {
        [Fact]
        public void EmbeddedContent_Construct_ValidContent()
        {
            // Arrange
            var contentId = new ContentIdentifier
            {
                ContentSourceProfileName = "ProfileName",
                MultipartId = new List<string> { "part1", "part2" },
                CanonicalId = "CanonicalId"
            };

            var contentParts = new List<EmbeddedContentPart>
            {
                new EmbeddedContentPart { Id = "Part1", Content = "Content1" , Embedding = new Embedding(){ } },
                new EmbeddedContentPart { Id = "Part2", Content = "Content2", Embedding = new Embedding(){ } }
            };

            // Act
            var embeddedContent = new EmbeddedContent
            {
                ContentId = contentId,
                ContentParts = contentParts
            };

            // Assert
            Assert.Equal(contentId, embeddedContent.ContentId);
            Assert.Equal(contentParts, embeddedContent.ContentParts);
        }

    }
}
