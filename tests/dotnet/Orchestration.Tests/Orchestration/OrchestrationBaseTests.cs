using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Orchestration;
using NSubstitute;
using Xunit;

namespace FoundationaLLM.Orchestration.Tests.Orchestration
{
    public class OrchestrationBaseTest
    {
        private readonly string _instanceId = "00000000-0000-0000-0000-000000000000";
        private readonly OrchestrationBase _orchestrationBase;
        private readonly ILLMOrchestrationService _orchestrationService = Substitute.For<ILLMOrchestrationService>();

        public OrchestrationBaseTest()
        {
            _orchestrationBase = new OrchestrationBase(_orchestrationService);
        }

        [Fact]
        public async Task GetCompletion_ShouldReturnNullCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest(){OperationId = Guid.NewGuid().ToString(), UserPrompt = ""};

            // Act
            var result = await _orchestrationBase.GetCompletion(completionRequest);

            // Assert
            Assert.Null(result);
        }
    }
}
