using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Services
{
    public class LangChainServiceTests
    {
        private readonly IOptions<LangChainServiceSettings> _options = Substitute.For<IOptions<LangChainServiceSettings>>();
        private readonly ILogger<LangChainService> _logger = Substitute.For<ILogger<LangChainService>>();
        private readonly IHttpClientFactoryService _httpClientFactoryService = Substitute.For<IHttpClientFactoryService>();
        private readonly LangChainService _langChainService;

        public LangChainServiceTests()
        {
            _langChainService = new LangChainService(_options, _logger, _httpClientFactoryService);
        }
    }
}
