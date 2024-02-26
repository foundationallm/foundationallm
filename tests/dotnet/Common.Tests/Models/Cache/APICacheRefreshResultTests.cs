using FoundationaLLM.Common.Models.Cache;

namespace FoundationaLLM.Common.Tests.Models.Cache
{
    public class APICacheRefreshResultTests
    {
        private APICacheRefreshResult _apiCacheRefreshResult = new APICacheRefreshResult();

        [Fact]
        public void APICacheRefreshResult_Detail_SetCorrectly()
        {
            // Arrange
            var expectedDetail = "TestDetail";
            _apiCacheRefreshResult.Detail = expectedDetail;

            // Assert
            Assert.Equal(expectedDetail, _apiCacheRefreshResult.Detail);
        }

        [Fact]
        public void APICacheRefreshResult_Success_SetCorrectly_True()
        {
            // Arrange
            _apiCacheRefreshResult.Success = true;

            // Assert
            Assert.True(_apiCacheRefreshResult.Success);
        }

        [Fact]
        public void APICacheRefreshResult_Success_SetCorrectly_False()
        {
            // Arrange
            _apiCacheRefreshResult.Success = false;

            // Assert
            Assert.False(_apiCacheRefreshResult.Success);
        }
    }
}
