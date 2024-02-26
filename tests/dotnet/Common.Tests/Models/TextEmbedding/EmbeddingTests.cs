using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Tests.Models.TextEmbedding
{
    public class EmbeddingTests
    {
        [Fact]
        public void Embedding_Constructor_Initializes_Vector_And_Length()
        {
            // Arrange
            float[] vector = new float[] { 1.0f, 2.0f, 3.0f };

            // Act
            var embedding = new Embedding(vector);

            // Assert
            Assert.Equal(vector.Length, embedding.Length);
            Assert.Equal(vector, embedding.Vector.ToArray());
        }
    }
}
