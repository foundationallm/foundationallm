using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Models.Configuration.Users;

namespace FoundationaLLM.Common.Models.Metadata
{
    /// <summary>
    /// Represents the name and privacy of a FoundationaLLM agent.
    /// Used as a reference in the <see cref="UserProfile"/> class.
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// The name of the agent.
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }
        /// <summary>
        /// Indicates whether the agent is private.
        /// </summary>
        [JsonProperty("private")]
        public bool Private { get; set; }
    }
}
