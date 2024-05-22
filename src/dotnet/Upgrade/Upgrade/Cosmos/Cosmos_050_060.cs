﻿using FoundationaLLM.Common.Models.Configuration.Instance;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Utility.Upgrade.Cosmos
{
    public class Cosmos_050_060 : CosmosUpgrade
    {
        public Cosmos_050_060(
            InstanceSettings settings,
            ILoggerFactory loggerFactory) : base(settings, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Cosmos_050_060>();

            SourceInstanceVersion = Version.Parse("0.4.0");
        }

        private ILogger<Cosmos_050_060> _logger;

        public void ConfigureDefaultValues() => base.ConfigureDefaultValues();

        public async override Task<object> UpgradeDoWorkAsync(object in_source) => null;

        public override Task<object> UpgradeProperties(object source) => Task.FromResult(source);
    }
}
