using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Collections.Immutable;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{

    public class ResourcePathTestData : TheoryData<string?, ImmutableList<string>, Dictionary<string, ResourceTypeDescriptor>, bool, bool>
    {
        ImmutableList<string> _allowedResourceProviders = ImmutableList<string>.Empty
            .Add("FoundationaLLM.Authorization");

        Dictionary<string, ResourceTypeDescriptor> _allowedResourceTypes = new()
        {
            { 
                "shapes", 
                new ResourceTypeDescriptor("shapes", typeof(object))
                {
                    AllowedTypes = [
                        new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [])
                    ],
                    Actions = [
                        new ResourceTypeAction("action1", false, true, []),
                        new ResourceTypeAction("action2", true, false, []),
                    ],
                    SubTypes = new()
                    {
                        {
                            "components",
                            new ResourceTypeDescriptor("components", typeof(object))
                            {
                                AllowedTypes = [
                                    new ResourceTypeAllowedTypes(HttpMethod.Get.Method, [], [], [])
                                ],
                                Actions = [
                                    new ResourceTypeAction("action3", false, true, []),
                                    new ResourceTypeAction("action4", true, false, []),
                                ],
                            }
                        }
                    }
                }
            }
        };

        public ResourcePathTestData()
        {
            AddInvalidResourcePath(null, true);
            AddInvalidResourcePath(string.Empty, true);
            AddInvalidResourcePath(" ", true);
            AddInvalidResourcePath("/", true);
            AddInvalidResourcePath("instances", true);
            AddInvalidResourcePath("/instances", true);
            AddInvalidResourcePath("/providers", true);
            AddInvalidResourcePath("/providers/FoundationaLLM.TestProvider", true);
            AddInvalidResourcePath("/instances/1", true);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/shapes/circle", true);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/forms/circle", true);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/parts", true);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/components/component1/parts", true);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/action1", false);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/action2", true);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/action1", true);
            AddInvalidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/components/component1/action3", true);

            AddValidResourcePath($"/instances/{Guid.NewGuid()}", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle", true);
            AddValidResourcePath($"/providers/FoundationaLLM.Authorization/shapes/circle", true);
            AddValidResourcePath($"/shapes/circle", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/components", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/components/component1", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/action1", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/action2", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/components/action3", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/components/component1/action4", true);
            AddValidResourcePath($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization/shapes/circle/components/action1", false);  // action1 is considered to be a resource.
        }

        private void AddInvalidResourcePath(string ?resourcePath, bool allowAction) =>
            Add(resourcePath, _allowedResourceProviders, _allowedResourceTypes, allowAction, false);

        private void AddValidResourcePath(string? resourcePath, bool allowAction) =>
            Add(resourcePath, _allowedResourceProviders, _allowedResourceTypes, allowAction, true);
    }

    public class ResourcePathResourceProviderTestData: TheoryData<string, bool>
    {
        public ResourcePathResourceProviderTestData()
        {
            Add($"/instances/{Guid.NewGuid()}", false);
            Add($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.InvalidResourceProvider", false);

            Add($"/instances/{Guid.NewGuid()}/providers/FoundationaLLM.Authorization", true);
            Add($"/providers/FoundationaLLM.Authorization", true);
        }
    }

    public class ResourcePathTests
    {
        [Theory]
        [ClassData(typeof(ResourcePathTestData))]
        public void ResourcePath_ParsedCorrectly(
            string resourcePath,
            ImmutableList<string> allowedResourceProviders,
            Dictionary<string, ResourceTypeDescriptor> allowedResourceTypes,
            bool allowAction,
            bool validPath)
        {
            if (validPath)
                Assert.NotNull(new ResourcePath(
                    resourcePath,
                    allowedResourceProviders,
                    allowedResourceTypes,
                    allowAction));
            else
                Assert.Throws<ResourceProviderException>(() => new ResourcePath(
                    resourcePath,
                    allowedResourceProviders,
                    allowedResourceTypes,
                    allowAction));
        }

        [Theory]
        [ClassData(typeof(ResourcePathResourceProviderTestData))]
        public void ResourcePath_ResourceProvider_IdentifiedCorrectly( string resourcePath, bool expectSuccess )
        {
            var success = ResourcePath.TryParseResourceProvider(resourcePath, out string? resourceProvider);
            if (expectSuccess)
            {
                Assert.True(success);
                Assert.Equal("FoundationaLLM.Authorization", resourceProvider);
            }
            else
            {
                Assert.False(success);
                Assert.Null(resourceProvider);
            }
        }
    }
}