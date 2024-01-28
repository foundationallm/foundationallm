using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// Configuration for the CXO agent.
    /// </summary>
    public class CXOConfiguration : SearchServiceConfiguration
    {
        /// <summary>
        /// The type of configuration. This value should not be changed.
        /// </summary>
        [JsonProperty("configuration_type")]
        public new string ConfigurationType = "cxo";
        /// <summary>
        /// Search filter elements.
        /// </summary>
        [JsonProperty("sources")]
        public string[]? Sources { get; set; }

        /// <summary>
        /// The vector database.
        /// </summary>
        [JsonProperty("retriever_mode")]
        public required string RetrieverMode { get; set; }

        /// <summary>
        /// The name of the CXO's company.
        /// </summary>
        [JsonProperty("company")]
        public required string Company { get; set; }
    }
}
