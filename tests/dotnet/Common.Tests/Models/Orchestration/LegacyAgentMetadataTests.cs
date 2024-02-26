using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class LegacyAgentMetadataTests
    {
        [Fact]
        public void LegacyAgentMetadata_Properties_Test()
        {
            // Arrange
            var metadata = new LegacyAgentMetadata();
            var promptPrefix = "Prefix";
            var promptSuffix = "Suffix";

            // Act
            metadata.PromptPrefix = promptPrefix;
            metadata.PromptSuffix = promptSuffix;

            // Assert
            Assert.Equal(promptPrefix, metadata.PromptPrefix);
            Assert.Equal(promptSuffix, metadata.PromptSuffix);
        }
    }
}
