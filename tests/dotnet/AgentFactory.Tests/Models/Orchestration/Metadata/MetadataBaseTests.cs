using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class MetadataBaseTests
    {
        private readonly string _name = "TestName";
        private readonly string _type = "TestType";
        private readonly string _description = "TestDescription";
        private readonly MetadataBase _metadataBase;

        public MetadataBaseTests()
        {
            _metadataBase = new MetadataBase
            {
                Name = _name,
                Type = _type,
                Description = _description
            };
        }

        [Fact]
        public void MetadataBase_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_name, _metadataBase.Name);
            Assert.Equal(_type, _metadataBase.Type);
            Assert.Equal(_description, _metadataBase.Description);
        }

        [Fact]
        public void MetadataBase_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_metadataBase);
            var deserializedMetadataBase = JsonConvert.DeserializeObject<MetadataBase>(serializedJson);

            // Assert
            Assert.Equal(_metadataBase.Name, deserializedMetadataBase?.Name);
            Assert.Equal(_metadataBase.Type, deserializedMetadataBase?.Type);
            Assert.Equal(_metadataBase.Description, deserializedMetadataBase?.Description);
        }
    }
}
