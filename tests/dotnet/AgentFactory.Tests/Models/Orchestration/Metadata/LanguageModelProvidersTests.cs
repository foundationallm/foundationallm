using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class LanguageModelProvidersTests
    {
        [Fact]
        public void LanguageModelProviders_Values_ShouldMatchExpected()
        {
            // Assert
            Assert.Equal("microsoft", LanguageModelProviders.MICROSOFT);
            Assert.Equal("openai", LanguageModelProviders.OPENAI);
        }
    }
}
