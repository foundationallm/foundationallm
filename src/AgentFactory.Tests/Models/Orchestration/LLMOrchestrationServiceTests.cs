using FoundationaLLM.AgentFactory.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration
{
    public class LLMOrchestrationServiceTests
    {
        [Fact]
        public void LLMOrchestrationService_Values_ShouldMatchExpected()
        {
            // Assert
            Assert.Equal("LangChain", LLMOrchestrationService.LangChain.ToString());
            Assert.Equal("SemanticKernel", LLMOrchestrationService.SemanticKernel.ToString());
        }
    }
}
