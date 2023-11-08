using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.Common.Models.Chat;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration
{
    public class LLMOrchestrationCompletionRequestTests
    {
        private readonly Agent _agent = new Agent { PromptTemplate = "TestPromptTemplate" };
        private readonly MetadataBase _dataSourceMetadata = new MetadataBase { Name = "TestDataSource" };
        private readonly LanguageModel _languageModel = new LanguageModel { Type = "TestType" };
        private readonly List<MessageHistoryItem> _messageHistory  = new List<MessageHistoryItem>{
            new MessageHistoryItem("Sender1", "Text1"),new MessageHistoryItem("Sender2", "Text2") 
        };
        private readonly LLMOrchestrationCompletionRequest _completionRequest;

        public LLMOrchestrationCompletionRequestTests()
        {
            _completionRequest = new LLMOrchestrationCompletionRequest
            {
                Agent = _agent,
                DataSourceMetadata = _dataSourceMetadata,
                LanguageModel = _languageModel,
                MessageHistory = _messageHistory
            };
        }

        [Fact]
        public void LLMOrchestrationCompletionRequest_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_agent, _completionRequest.Agent);
            Assert.Equal(_dataSourceMetadata, _completionRequest.DataSourceMetadata);
            Assert.Equal(_languageModel, _completionRequest.LanguageModel);
            Assert.Equal(_messageHistory, _completionRequest.MessageHistory);
        }

        [Fact]
        public void LLMOrchestrationCompletionRequest_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_completionRequest);
            var deserializedCompletionRequest = JsonConvert.DeserializeObject<LLMOrchestrationCompletionRequest>(serializedJson);

            // Assert
            Assert.Equal(_agent.PromptTemplate, deserializedCompletionRequest?.Agent?.PromptTemplate);
            Assert.Equal(_dataSourceMetadata.Name, deserializedCompletionRequest?.DataSourceMetadata?.Name);
            Assert.Equal(_languageModel.Type, deserializedCompletionRequest?.LanguageModel?.Type);
            Assert.Equal(_messageHistory[0].Sender, deserializedCompletionRequest?.MessageHistory[0].Sender);
            Assert.Equal(_messageHistory[1].Text, deserializedCompletionRequest?.MessageHistory[1].Text);
        }
    }
}
