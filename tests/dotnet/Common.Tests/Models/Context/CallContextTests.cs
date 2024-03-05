using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using NSubstitute;

namespace FoundationaLLM.Common.Tests.Models.Context
{
    public class CallContextTests
    {
        [Fact]
        public void TestCurrentUserIdentityWithNSubstitute()
        {
            // Arrange
            var callContext = new CallContext();
            var userIdentity = Substitute.For<UnifiedUserIdentity>();
            userIdentity.Name = "TestName";
            userIdentity.Username = "TestUsername";
            userIdentity.UPN = "TestUPN";

            // Act
            callContext.CurrentUserIdentity = userIdentity;

            // Assert
            Assert.Equal("TestName", callContext.CurrentUserIdentity.Name);
            Assert.Equal("TestUsername", callContext.CurrentUserIdentity.Username);
            Assert.Equal("TestUPN", callContext.CurrentUserIdentity.UPN);
        }

        [Fact]
        public void TestDefaultCurrentUserIdentity()
        {
            // Arrange
            var callContext = new CallContext();

            // Assert
            Assert.Null(callContext.CurrentUserIdentity);
        }
    }
}
