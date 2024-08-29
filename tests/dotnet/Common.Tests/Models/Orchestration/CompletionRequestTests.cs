using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration.Request;

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
