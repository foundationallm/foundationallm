﻿using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Utility.Upgrade.Vectorization.ContentSource;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FoundationaLLM.Utility.Upgrade.Models._040;
using FoundationaLLM.Utility.Upgrade.Models._050;
using FoundationaLLM.Utility.Upgrade.Models._051;
using FoundationaLLM.Utility.Upgrade.Models._060;

namespace FoundationaLLM.Utility.Upgrade.Vectorization.Embedding
{
    public class EmbeddingProfile_050_060 : ContentSourceProfileUpgrade
    {
        public EmbeddingProfile_050_060(BlobStorageService blobStorageService,
            InstanceSettings settings,
            ILoggerFactory loggerFactory) : base(blobStorageService, settings, loggerFactory)
        {
            _blobStorageService = blobStorageService;
            _logger = loggerFactory.CreateLogger<EmbeddingProfile_050_060>();

            TypeName = "ContentSourceProfile";

            SourceInstanceVersion = Version.Parse("0.5.0");

            SourceType = typeof(TextEmbeddingProfile050);
            TargetType = typeof(TextEmbeddingProfile060);
        }

        private ILogger<EmbeddingProfile_050_060> _logger;

        public void ConfigureDefaultValues() => base.ConfigureDefaultValues();

        public override Task<Dictionary<string, string>> LoadArtifacts() => base.LoadArtifacts();

        public async override Task<object> UpgradeDoWorkAsync(object in_agent)
        {
            ConfigureDefaultValues();

            string strAgent = JsonSerializer.Serialize(in_agent);

            TextEmbeddingProfile050 source = JsonSerializer.Deserialize<TextEmbeddingProfile050>(strAgent);

            TextEmbeddingProfile060 target = JsonSerializer.Deserialize<TextEmbeddingProfile060>(strAgent);

            if (source.Version == SourceInstanceVersion)
            {
                SetDefaultValues(target);

                target.Version = Version.Parse("0.6.0");

                _logger.LogInformation($"Upgraded {TypeName} {source.Name} from version {source.Version} to version {target.Version}");
            }

            return target;
        }

        public override Task<object> UpgradeProperties(object agent) => Task.FromResult(agent);
    }
}
