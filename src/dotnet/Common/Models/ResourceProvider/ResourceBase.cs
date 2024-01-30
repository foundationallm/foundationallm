using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.ResourceProvider
{
    /// <summary>
    /// Basic properties for all resources.
    /// </summary>
    public class ResourceBase
    {
        /// <summary>
        /// The name of the resource.
        /// </summary>
        [JsonProperty("name")]
        public required string Name { get; set; }
        /// <summary>
        /// The type of the resource.
        /// </summary>
        [JsonProperty("type")]
        public required string Type { get; set; }
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonProperty("object_id")]
        public required string ObjectId { get; set; }
        /// <summary>
        /// The description of the resource.
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }
    }
}
