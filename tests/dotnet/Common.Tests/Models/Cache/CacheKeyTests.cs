using FoundationaLLM.Common.Models.Cache;

namespace FoundationaLLM.Common.Tests.Models.Cache
{
    public class CacheKeyTests
    {
        [Fact]
        public void CacheKey_Equals_ReturnsTrue_WhenEqual()
        {
            // Arrange
            var name = "TestName";
            var category = "TestCategory";
            var cacheKey1 = new CacheKey(name, category);
            var cacheKey2 = new CacheKey(name, category);

            // Act
            var equals = cacheKey1.Equals(cacheKey2);

            // Assert
            Assert.True(equals);
        }

        [Fact]
        public void CacheKey_Equals_ReturnsFalse_WhenNotEqual()
        {
            // Arrange
            var cacheKey1 = new CacheKey("Name1", "Category1");
            var cacheKey2 = new CacheKey("Name2", "Category2");

            // Act
            var equals = cacheKey1.Equals(cacheKey2);

            // Assert
            Assert.False(equals);
        }

        [Fact]
        public void CacheKey_GetHashCode_ReturnsSameValue_WhenEqual()
        {
            // Arrange
            var name = "TestName";
            var category = "TestCategory";
            var cacheKey1 = new CacheKey(name, category);
            var cacheKey2 = new CacheKey(name, category);

            // Act
            var hashCode1 = cacheKey1.GetHashCode();
            var hashCode2 = cacheKey2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        [Fact]
        public void CacheKey_GetHashCode_ReturnsDifferentValue_WhenNotEqual()
        {
            // Arrange
            var cacheKey1 = new CacheKey("Name1", "Category1");
            var cacheKey2 = new CacheKey("Name2", "Category2");

            // Act
            var hashCode1 = cacheKey1.GetHashCode();
            var hashCode2 = cacheKey2.GetHashCode();

            // Assert
            Assert.NotEqual(hashCode1, hashCode2);
        }
    }
}
