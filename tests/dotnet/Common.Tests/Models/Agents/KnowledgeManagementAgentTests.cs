using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Common.Tests.Models.Agents
{
    public class KnowledgeManagementAgentTests
    {
        private GenericAgent _knowledgeManagementAgent = new GenericAgent()
        {
            Name = "Test_agent",
            ObjectId = "Test_objectid",
            Type = AgentTypes.GenericAgent
        };

        [Fact]
        public void KnowledgeManagementAgent_Type_IsKnowledgeManagement()
        {
            // Assert
            Assert.Equal(AgentTypes.GenericAgent, _knowledgeManagementAgent.Type);
        }
    }
}
