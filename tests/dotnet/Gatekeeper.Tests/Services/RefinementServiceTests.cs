using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Gatekeeper.Tests.Services
{
    public class RefinementServiceTests
    {
        private readonly RefinementService _testedService;

        private readonly IOptions<RefinementServiceSettings> _options = Substitute.For<IOptions<RefinementServiceSettings>>();
        private readonly ILogger<RefinementService> _logger = Substitute.For<ILogger<RefinementService>>();

        public RefinementServiceTests()
        {
            _testedService = new RefinementService(_options, _logger);
        }
    }
}
