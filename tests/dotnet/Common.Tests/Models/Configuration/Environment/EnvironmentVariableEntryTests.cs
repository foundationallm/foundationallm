using FoundationaLLM.Common.Models.Configuration.Environment;

namespace FoundationaLLM.Common.Tests.Models.Configuration.Environment
{
    public class EnvironmentVariableEntryTests
    {
        [Fact]
        public void EnvironmentVariableEntry_Properties_SetCorrectly()
        {
            // Arrange
            string name = "test_variable";
            string description = "Test environment variable";
            string defaultValue = "default_value";

            // Act
            var envVariableEntry = new EnvironmentVariableEntry(name, description, defaultValue);

            // Assert
            Assert.Equal(name, envVariableEntry.Name);
            Assert.Equal(description, envVariableEntry.Description);
            Assert.Equal(defaultValue, envVariableEntry.DefaultValue);
        }

        [Fact]
        public void EnvironmentVariableEntry_DefaultValue_NullIfNotProvided()
        {
            // Arrange
            string name = "test_variable";
            string description = "Test environment variable";

            // Act
            var envVariableEntry = new EnvironmentVariableEntry(name, description);

            // Assert
            Assert.Null(envVariableEntry.DefaultValue);
        }
    }
}
