using FoundationaLLM.Common.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FoundationaLLM.Common.Tests.Settings
{
    public class CommonJsonSerializerSettingsTests
    {
        [Fact]
        public void GetJsonSerializerSettings_ShouldReturnValidJsonSerializerSettings()
        {
            // Arrange
            var expectedSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Act
            var actualSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();

            // Assert
            Assert.Equal(expectedSettings.ContractResolver.GetType(), actualSettings.ContractResolver?.GetType());
        }
    }
}
