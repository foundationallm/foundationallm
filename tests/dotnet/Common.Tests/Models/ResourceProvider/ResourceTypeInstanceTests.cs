﻿using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceTypeInstanceTests
    {
        [Fact]
        public void ResourceTypeInstance_InitializeProperty()
        {
            // Arrange
            string expectedResourceType = "ResourceType";

            // Act
            var instance = new ResourceTypeInstance(expectedResourceType);

            // Assert
            Assert.Equal(expectedResourceType, instance.ResourceType);
            Assert.Null(instance.ResourceId);
            Assert.Null(instance.Action);
        }
    }
}
