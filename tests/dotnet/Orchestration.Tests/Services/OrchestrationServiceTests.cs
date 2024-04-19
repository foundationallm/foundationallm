using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace FoundationaLLM.Orchestration.Tests.Services
{
    public class OrchestrationServiceTests
    {
        private readonly IEnumerable<ILLMOrchestrationService> _orchestrationServices =
        [
            Substitute.For<ILLMOrchestrationService>(),
            Substitute.For<ILLMOrchestrationService>()
        ];
        private readonly ILogger<OrchestrationService> _logger = Substitute.For<ILogger<OrchestrationService>>();
        private readonly OrchestrationService _orchestrationService;
        private IEnumerable<IResourceProviderService> _resourceProviderServices = new List<IResourceProviderService>
        {
            Substitute.For<IResourceProviderService>()
        };
        private readonly ICallContext _callContext = Substitute.For<ICallContext>();
        private readonly ILoggerFactory _loggerFactory =  Substitute.For<ILoggerFactory>();
        private readonly IConfiguration _configuration = Substitute.For<IConfiguration>();


        public OrchestrationServiceTests()
        {
            _orchestrationService = new OrchestrationService(
                _resourceProviderServices,
                _orchestrationServices,
                _callContext,
                _configuration,
                _loggerFactory
            );
        }

        [Fact]
        public async Task GetCompletion_ValidCompletionRequest_ReturnsCompletionResponse()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                UserPrompt = "TestPrompt"
            };

            // Act
            var result = await _orchestrationService.GetCompletion(completionRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(completionRequest.UserPrompt, result.UserPrompt);
        }

        [Fact]
        public async Task GetCompletion_ExceptionThrown_ReturnsErrorResponse()
        {
            // Act 
            var result = await _orchestrationService.GetCompletion(new CompletionRequest() { UserPrompt = "Error" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal("A problem on my side prevented me from responding.", result.Completion);
        }
    }
}
