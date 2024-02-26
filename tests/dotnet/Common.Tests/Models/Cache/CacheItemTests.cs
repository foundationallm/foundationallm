using FoundationaLLM.Common.Models.Cache;

namespace FoundationaLLM.Common.Tests.Models.Cache
{
    public class CacheItemTests
    {
        private CacheItem _cacheItem = new CacheItem() { Value = "Test_cache"};

        [Fact]
        public void CacheItem_Value_SetCorrectly()
        {
            // Arrange
            var expectedValue = "TestValue";
            _cacheItem.Value = expectedValue;

            // Assert
            Assert.Equal(expectedValue, _cacheItem.Value);
        }

        [Fact]
        public void CacheItem_IsExpired_ReturnsFalse_WhenExpirationTimeUtcIsNull()
        {
            // Arrange
            _cacheItem.ExpirationTimeUtc = null;

            // Assert
            Assert.False(_cacheItem.IsExpired);
        }

        [Fact]
        public void CacheItem_IsExpired_ReturnsFalse_WhenExpirationTimeUtcIsFuture()
        {
            // Arrange
            _cacheItem.ExpirationTimeUtc = DateTime.UtcNow.AddMinutes(10);

            // Assert
            Assert.False(_cacheItem.IsExpired);
        }

        [Fact]
        public void CacheItem_IsExpired_ReturnsTrue_WhenExpirationTimeUtcIsPast()
        {
            // Arrange
            _cacheItem.ExpirationTimeUtc = DateTime.UtcNow.AddMinutes(-10);

            // Assert
            Assert.True(_cacheItem.IsExpired);
        }
    }
}
