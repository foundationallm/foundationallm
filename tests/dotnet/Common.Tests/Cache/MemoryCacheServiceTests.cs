using FoundationaLLM.Common.Models.Cache;
using FoundationaLLM.Common.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Cache
{
    public class MemoryCacheServiceTests
    {
        [Fact]
        public void Get_Item_From_Cache()
        {
            var cache = new MemoryCacheService(
                Substitute.For<ILogger<MemoryCacheService>>());

            cache.Set<string>(new CacheKey("xyz", "agent"), "Hello world", TimeSpan.FromHours(1));

            var item = cache.Get<string>(new CacheKey("xyz", "agent"));

            Assert.Equal("Hello world", item);
        }

        [Fact]
        public void Remove_Item_From_Cache()
        {
            var cache = new MemoryCacheService(
                Substitute.For<ILogger<MemoryCacheService>>());

            cache.Set<string>(new CacheKey("xyz", "agent"), "Hello world", TimeSpan.FromHours(1));

            cache.Remove(new CacheKey("xyz", string.Empty));

            Assert.Equal(0, cache.GetItemsCount("agent"));
        }

        [Fact]
        public void Remove_Category_From_Cache()
        {
            var cache = new MemoryCacheService(
                Substitute.For<ILogger<MemoryCacheService>>());

            cache.Set<string>(new CacheKey("xyz1", "agent"), "Hello world", TimeSpan.FromHours(1));
            cache.Set<string>(new CacheKey("xyz2", "agent"), "Hello world", TimeSpan.FromHours(1));

            Assert.Equal(2, cache.GetItemsCount("agent"));

            cache.RemoveByCategory(new CacheKey(string.Empty, "agent"));

            Assert.Equal(0, cache.GetItemsCount("agent"));
        }
    }
}
