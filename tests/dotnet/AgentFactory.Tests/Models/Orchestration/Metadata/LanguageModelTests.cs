using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class LanguageModelTests
    {
        private readonly string _type = "TestType";
        private readonly string _provider = "TestProvider";
        private readonly float _temperature = 0.5f;
        private readonly bool _useChat = false;
        private readonly LanguageModel _languageModel;

        public LanguageModelTests()
        {
            _languageModel = new LanguageModel
            {
                Type = _type,
                Provider = _provider,
                Temperature = _temperature,
                UseChat = _useChat
            };
        }

        [Fact]
        public void LanguageModel_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_type, _languageModel.Type);
            Assert.Equal(_provider, _languageModel.Provider);
            Assert.Equal(_temperature, _languageModel.Temperature);
            Assert.Equal(_useChat, _languageModel.UseChat);
        }

        [Fact]
        public void LanguageModel_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_languageModel);
            var deserializedLanguageModel = JsonConvert.DeserializeObject<LanguageModel>(serializedJson);

            // Assert
            Assert.Equal(_languageModel.Type, deserializedLanguageModel?.Type);
            Assert.Equal(_languageModel.Provider, deserializedLanguageModel?.Provider);
            Assert.Equal(_languageModel.Temperature, deserializedLanguageModel?.Temperature);
            Assert.Equal(_languageModel.UseChat, deserializedLanguageModel?.UseChat);
        }
    }
}
