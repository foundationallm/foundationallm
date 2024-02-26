using FoundationaLLM.Common.Models.Agents;

namespace FoundationaLLM.Common.Tests.Models.Agents
{
    public class InternalContextAgentTests
    {
        private InternalContextAgent _internalContextAgent = new InternalContextAgent() 
            { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.InternalContext };

        [Fact]
        public void InternalContextAgent_Type_IsInternalContext()
        {
            // Assert
            Assert.Equal(AgentTypes.InternalContext, _internalContextAgent.Type);
        }
    }
}
