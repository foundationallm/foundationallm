using FoundationaLLM.Common.Models.Cache;
using FoundationaLLM.Common.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FoundationaLLM.Common.Tests.Services
{
    public class MemoryCacheServiceTests
    {
        [Fact]
        public void Get_Item_From_Cache()
        {
            var cache = new MemoryCacheService(
                Substitute.For<ILogger<MemoryCacheService>>());

            cache.Set(new CacheKey("xyz", "agent"), "Hello world", TimeSpan.FromHours(1));

            var item = cache.Get<string>(new CacheKey("xyz", "agent"));

            Assert.Equal("Hello world", item);
        }

        [Fact]
        public void Remove_Item_From_Cache()
        {
            var cache = new MemoryCacheService(
                Substitute.For<ILogger<MemoryCacheService>>());

            cache.Set(new CacheKey("xyz", "agent"), "Hello world", TimeSpan.FromHours(1));

            cache.Remove(new CacheKey("xyz", "agent"));

            Assert.Equal(0, cache.GetItemsCount("agent"));
        }

        [Fact]
        public void Remove_Category_From_Cache()
        {
            var cache = new MemoryCacheService(
                Substitute.For<ILogger<MemoryCacheService>>());

            cache.Set(new CacheKey("xyz1", "agent"), "Hello world", TimeSpan.FromHours(1));
            cache.Set(new CacheKey("xyz2", "agent"), "Hello world", TimeSpan.FromHours(1));

            Assert.Equal(2, cache.GetItemsCount("agent"));

            cache.RemoveByCategory(new CacheKey(string.Empty, "agent"));

            Assert.Equal(0, cache.GetItemsCount("agent"));
        }
    }
}
