using FoundationaLLM.Common.Models.Conversation;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Reflection;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class SessionTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            var session = new Conversation { Name = "Test", SessionId = "1" };

            Assert.Equal(nameof(Conversation), session.Type);
            Assert.Equal(0, session.TokensUsed);
            Assert.Equal("New Chat", session.Name);
            Assert.NotNull(session.Messages);
            Assert.Empty(session.Messages);
        }

        [Fact]
        public void AddMessage_ShouldAddMessageToMessagesList()
        {
            // Arrange
            var session = new Conversation { Name = "Test", SessionId = "1" };
            var message = new Message
            {
                SessionId = "1",
                Sender = "sender1",
                Tokens = 0,
                Text = "The message",
                Vector = null,
                Rating = null,
                UPN = "test@foundationallm.ai"
            };

            // Act
            session.AddMessage(message);

            // Assert
            Assert.Contains(message, session.Messages);
        }

        [Fact]
        public void UpdateMessage_ShouldUpdateExistingMessageInMessagesList()
        {
            // Arrange
            var session = new Conversation { Name = "Test", SessionId = "1" };
            var initialMessage = new Message
            {
                SessionId = "1",
                Sender = "sender1",
                Tokens = 0,
                Text = "The message",
                Vector = null,
                Rating = null,
                UPN = "test@foundationallm.ai"
            };
            session.AddMessage(initialMessage);

            var updatedMessage = new Message
            {
                SessionId = "1",
                Sender = "sender1",
                Tokens = 0,
                Text = "The message",
                Vector = null,
                Rating = null,
                UPN = "test@foundationallm.ai"
            };
            updatedMessage.Id = initialMessage.Id;

            // Act
            session.UpdateMessage(updatedMessage);

            // Assert
            Assert.DoesNotContain(initialMessage, session.Messages);
            Assert.Contains(updatedMessage, session.Messages);
        }
    }
}
