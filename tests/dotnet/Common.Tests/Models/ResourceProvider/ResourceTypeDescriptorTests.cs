using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Tests.Models.ResourceProvider
{
    public class ResourceTypeDescriptorTests
    {
        [Fact]
        public void ResourceTypeDescriptor_InitializeProperty()
        {
            // Arrange
            string expectedResourceType = "ResourceType";

            // Act
            var descriptor = new ResourceTypeDescriptor(expectedResourceType, typeof(object));

            // Assert
            Assert.Equal(expectedResourceType, descriptor.ResourceTypeName);
            Assert.NotNull(descriptor.Actions);
            Assert.NotNull(descriptor.AllowedTypes);
            Assert.NotNull(descriptor.SubTypes);
        }
    }

    public class ResourceTypeActionTests
    {
        [Fact]
        public void ResourceTypeAction_Initialize_CheckProperties()
        {
            // Arrange
            string expectedName = "ActionName";
            bool expectedAllowedOnResource = true;
            bool expectedAllowedOnResourceType = false;
            var allowedTypes = new List<ResourceTypeAllowedTypes>();

            // Act
            var action = new ResourceTypeAction(expectedName, expectedAllowedOnResource,
                                                 expectedAllowedOnResourceType, allowedTypes);

            // Assert
            Assert.Equal(expectedName, action.Name);
            Assert.Equal(expectedAllowedOnResource, action.AllowedOnResource);
            Assert.Equal(expectedAllowedOnResourceType, action.AllowedOnResourceType);
            Assert.NotNull(action.AllowedTypes);
        }
    }

    public class ResourceTypeAllowedTypesTests
    {
        [Fact]
        public void ResourceTypeAllowedTypes_Initialize_CheckProperties()
        {
            // Arrange
            string expectedHttpMethod = "GET";
            var allowedParameterTypes = new Dictionary<string, Type>();
            var allowedBodyTypes = new List<Type>();
            var allowedReturnTypes = new List<Type>();

            // Act
            var allowedTypes = new ResourceTypeAllowedTypes(expectedHttpMethod, allowedParameterTypes,
                                                            allowedBodyTypes, allowedReturnTypes);

            // Assert
            Assert.Equal(expectedHttpMethod, allowedTypes.HttpMethod);
            Assert.NotNull(allowedTypes.AllowedParameterTypes);
            Assert.NotNull(allowedTypes.AllowedBodyTypes);
            Assert.NotNull(allowedTypes.AllowedReturnTypes);
        }
    }
}
