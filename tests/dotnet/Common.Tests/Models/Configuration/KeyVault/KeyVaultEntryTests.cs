using FoundationaLLM.Common.Models.Configuration.KeyVault;

namespace FoundationaLLM.Common.Tests.Models.Configuration.KeyVault
{
    public class KeyVaultEntryTests
    {
        [Fact]
        public void KeyVaultSecretEntry_Properties_SetCorrectly()
        {
            // Arrange
            string expectedSecretName = "MySecret";
            string expectedMinimumVersion = "1.0";
            string expectedDescription = "This is a secret";

            // Act
            var secretEntry = new KeyVaultSecretEntry(
                expectedSecretName,
                expectedMinimumVersion,
                expectedDescription);

            // Assert
            Assert.Equal(expectedSecretName, secretEntry.SecretName);
            Assert.Equal(expectedMinimumVersion, secretEntry.MinimumVersion);
            Assert.Equal(expectedDescription, secretEntry.Description);
        }
    }
}
