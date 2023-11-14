using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class LanguageModelTypesTests
    {
        [Fact]
        public void LanguageModelTypes_Values_ShouldMatchExpected()
        {
            // Assert
            Assert.Equal("openai", LanguageModelTypes.OPENAI);
        }
    }
}
