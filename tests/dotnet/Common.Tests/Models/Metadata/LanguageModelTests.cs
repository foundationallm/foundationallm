using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Models.Metadata;

namespace FoundationaLLM.Common.Tests.Models.Metadata
{
    public class LanguageModelTests
    {
        [Fact]
        public void LanguageModel_Properties_Initialized_Correctly()
        {
            // Arrange
            var languageModel = new LanguageModel();

            // Assert
            Assert.Null(languageModel.Type);
            Assert.Equal(LanguageModelProviders.MICROSOFT, languageModel.Provider);
            Assert.Equal(0f, languageModel.Temperature);
            Assert.True(languageModel.UseChat);
            Assert.Null(languageModel.ApiEndpoint);
            Assert.Null(languageModel.ApiKey);
            Assert.Null(languageModel.ApiVersion);
            Assert.Null(languageModel.Version);
            Assert.Null(languageModel.Deployment);
        }
    }
}
