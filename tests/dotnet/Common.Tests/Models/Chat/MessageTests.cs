using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.Common.Tests.Models.Chat
{
    public class MessageTests
    {
        public static IEnumerable<object?[]> GetInvalidFields()
        {
            yield return new object?[] { null, "sender1", null, "The message", null, null, null };
            yield return new object?[] { "", "sender1", null, "The message", null, null, null };
            yield return new object?[] { "1", null, null, "The message", null, null, null };
            yield return new object?[] { "1", "", null, "The message", null, null, null };
            yield return new object?[] { "1", "sender1", null, null, null, null, null };
            yield return new object?[] { "1", "sender1", null, "", null, null, null };
            yield return new object?[] { "1", "sender1", null, "The message", new float[0], null, null };
            yield return new object?[] { "1", "sender1", null, "The message", new float[] { 1, 2, 3 }, null, null };
        }

        public static IEnumerable<object?[]> GetValidFields()
        {
            yield return new object?[] { "1", "sender1", null, "The message", null, null, null };
            yield return new object?[] { "1", "sender1", null, "The message", Enumerable.Range(0, 1536).Select(x => (float)x).ToArray(), null, null };
        }

        //[Theory]
        //[MemberData(nameof(GetInvalidFields))]
        //public void Create_Message_FailsWithInvalidValues(string sessionId, string sender, int? tokens, string text,
        //    float[]? vector, bool? rating, string upn)
        //{
        //    Assert.Throws<Exception>(() => CreateMessage(sessionId, sender, tokens, text, vector, rating, upn));
        //}

        [Theory]
        [MemberData(nameof(GetValidFields))]
        public void Create_Message_SucceedsWithValidValues(string sessionId, string sender, int? tokens, string text,
            float[]? vector, bool? rating, string upn)
        {
            //Act
            var exception = Record.Exception(() => CreateMessage(sessionId, sender, tokens, text, vector, rating, upn));

            //Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var expectedSessionId = "Session_1";
            var expectedSender = "Sender_1";
            int? expectedTokens = 10;
            var expectedText = "Text";
            float[] expectedVector = new float[] { 1,2,3 };
            bool? expectedRating = true;
            var expectedUpn = "test@foundationallm.ai";

            // Act
            var message = CreateMessage(
                expectedSessionId,
                expectedSender,
                expectedTokens,
                expectedText,
                expectedVector,
                expectedRating,
                expectedUpn
            );

            // Assert
            Assert.NotEmpty(message.Id);
            Assert.Equal("Message", message.Type);
            Assert.Equal(expectedSessionId, message.SessionId);
            Assert.Equal(expectedSender, message.Sender);
            Assert.Equal(expectedTokens, message.Tokens);
            Assert.Equal(expectedText, message.Text);
            Assert.Equal(expectedRating, message.Rating);
            Assert.Equal(expectedVector, message.Vector);
            Assert.Equal(expectedUpn, message.UPN);
        }

        public Message CreateMessage(string sessionId, string sender, int? tokens, string text,
            float[]? vector, bool? rating, string upn)
        {
            return new Message(sessionId, sender, tokens, text, vector, rating, upn);
        }
    }
}
