using FoundationaLLM.Common.Models.Conversation;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class SessionTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            var session = new Conversation();

            Assert.NotNull(session.Id);
            Assert.Equal(nameof(Conversation), session.Type);
            Assert.Equal(session.Id, session.SessionId);
            Assert.Equal(0, session.TokensUsed);
            Assert.Equal("New Chat", session.Name);
            Assert.NotNull(session.Messages);
            Assert.Empty(session.Messages);
        }

        [Fact]
        public void AddMessage_ShouldAddMessageToMessagesList()
        {
            // Arrange
            var session = new Conversation();
            var message = new Message("1", "sender1", null, "The message", null, null, "test@foundationallm.ai");

            // Act
            session.AddMessage(message);

            // Assert
            Assert.Contains(message, session.Messages);
        }

        [Fact]
        public void UpdateMessage_ShouldUpdateExistingMessageInMessagesList()
        {
            // Arrange
            var session = new Conversation();
            var initialMessage = new Message("1", "sender1", null, "The message", null, null, "test@foundationallm.ai");
            session.AddMessage(initialMessage);

            var updatedMessage = new Message("1", "sender1", null, "The message updated", null, null, "test@foundationallm.ai");
            updatedMessage.Id = initialMessage.Id;

            // Act
            session.UpdateMessage(updatedMessage);

            // Assert
            Assert.DoesNotContain(initialMessage, session.Messages);
            Assert.Contains(updatedMessage, session.Messages);
        }
    }
}
