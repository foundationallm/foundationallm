﻿using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class CompletionRequestTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedPrompt = "Generate some text";
            var expectedMessageHistory = new List<MessageHistoryItem>
            {
                new MessageHistoryItem("Sender_1", "Test"),
                new MessageHistoryItem("Sender_2", "Test")
            };

            // Act
            var completionRequest = new CompletionRequest
            {
                OperationId = Guid.NewGuid().ToString(),
                UserPrompt = expectedPrompt,
                MessageHistory = expectedMessageHistory
            };

            // Assert
            Assert.Equal(expectedPrompt, completionRequest.UserPrompt);
            Assert.Equal(expectedMessageHistory, completionRequest.MessageHistory);
        }
    }
}
