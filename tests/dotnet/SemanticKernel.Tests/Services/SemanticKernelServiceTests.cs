﻿using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using FoundationaLLM.SemanticKernel.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using NSubstitute;

namespace FoundationaLLM.SemanticKernel.Tests.Services
{
    public class SemanticKernelServiceTests
    {
        private readonly SemanticKernelService _testedService;

        private readonly ISystemPromptService _systemPromptService = Substitute.For<ISystemPromptService>();
        private readonly IEnumerable<IMemorySource> _memorySources = Substitute.For<IEnumerable<IMemorySource>>();
        private readonly IOptions<SemanticKernelServiceSettings> _options = Substitute.For<IOptions<SemanticKernelServiceSettings>>();
        private readonly IOptions<AzureCognitiveSearchMemorySourceSettings> _cognitiveSearchMemorySourceSettings = Substitute.For<IOptions<AzureCognitiveSearchMemorySourceSettings>>();
        private readonly ILogger<SemanticKernelService> _logger = Substitute.For<ILogger<SemanticKernelService>>();
        private readonly ILoggerFactory _loggerFactory = Substitute.For<ILoggerFactory>();

        public SemanticKernelServiceTests()
        {
            _testedService = new SemanticKernelService(_systemPromptService, _memorySources, _options, _cognitiveSearchMemorySourceSettings, _logger, _loggerFactory);
        }

        #region GetCompletion

        [Fact]
        public async Task GetCompletion_ShouldReturnACompletionForAPrompt()
        {
            // Arrange
            var userPrompt = "This is a prompt.";
            var promptName = "prompt";

            var expectedCompletion = "This is a completion.";

            _systemPromptService.GetPrompt(promptName).Returns(expectedCompletion);

            // Act
            var actualCompletion = await _testedService.GetCompletion(userPrompt, new List<MessageHistoryItem>());

            // Assert
            Assert.Equal(expectedCompletion, actualCompletion);
        }

        #endregion

        #region GetSummary

        /// TODO: Add tests for GetSummary

        #endregion

        #region AddMemory

        /// TODO: Add tests for AddMemory

        #endregion

        #region RemoveMemory

        /// TODO: Add tests for RemoveMemory

        #endregion
    }
}
