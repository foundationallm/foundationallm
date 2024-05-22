﻿using FoundationaLLM.Utility.Upgrade.Models._040;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Utility.Upgrade.Models._050
{
    public class CXODataSource050 : DataSourceBase050
    {
        /// <summary>
        /// Search Service configuration settings.
        /// </summary>
        [JsonPropertyName("configuration")]
        public CXOConfiguration040? Configuration { get; set; }
    }
}
