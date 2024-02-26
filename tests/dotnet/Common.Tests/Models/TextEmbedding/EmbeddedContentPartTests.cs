using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Tests.Models.TextEmbedding
{
    public class EmbeddedContentPartTests
    {
        [Fact]
        public void EmbeddedContentPart_Properties_AreInitializedCorrectly()
        {
            // Arrange
            string id = "Test_Id";
            string content = "Test_Content";
            float[] vector = new float[] { 1.0f, 2.0f, 3.0f };
            Embedding embedding = new Embedding(vector);

            // Act
            var embeddedContentPart = new EmbeddedContentPart
            {
                Id = id,
                Content = content,
                Embedding = embedding
            };

            // Assert
            Assert.Equal(id, embeddedContentPart.Id);
            Assert.Equal(content, embeddedContentPart.Content);
            Assert.Equal(embedding, embeddedContentPart.Embedding);
        }
    }
}
