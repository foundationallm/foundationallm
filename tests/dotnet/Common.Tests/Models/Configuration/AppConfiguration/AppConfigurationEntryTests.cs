using FoundationaLLM.Common.Models.Configuration.AppConfiguration;

namespace FoundationaLLM.Common.Tests.Models.Configuration.AppConfiguration
{
    public class AppConfigurationEntryTests
    {
        [Fact]
        public void AppConfigurationEntry_Properties_SetCorrectly()
        {
            // Arrange
            string key = "test_key";
            string minimumVersion = "1.0";
            string keyVaultSecretName = "test_secret";
            string description = "Test description";
            string defaultValue = "default_value";
            bool canBeEmpty = true;
            string contentType = "application/json";
            object sampleObject = new { Name = "Sample" };

            // Act
            var appConfigEntry = new AppConfigurationEntry(
                key,
                minimumVersion,
                keyVaultSecretName,
                description,
                defaultValue,
                contentType,
                sampleObject,
                canBeEmpty
            );

            // Assert
            Assert.Equal(key, appConfigEntry.Key);
            Assert.Equal(minimumVersion, appConfigEntry.MinimumVersion);
            Assert.Equal(keyVaultSecretName, appConfigEntry.KeyVaultSecretName);
            Assert.Equal(description, appConfigEntry.Description);
            Assert.Equal(defaultValue, appConfigEntry.DefaultValue);
            Assert.Equal(canBeEmpty, appConfigEntry.CanBeEmpty);
            Assert.Equal(contentType, appConfigEntry.ContentType);
            Assert.Equal(sampleObject, appConfigEntry.SampleObject);
        }
    }
}
