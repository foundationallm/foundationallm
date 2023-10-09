using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security
{
    public class RoleAssignment
    {
        public RoleAssignment() { }

        [JsonProperty(PropertyName="name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "role")]
        public Role Role { get; set; }

        [JsonProperty(PropertyName = "principal")]
        public Principal Principal { get; set; }

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set; }

        [JsonProperty(PropertyName = "tenant")]
        public string Tenant { get; set; }

        [JsonProperty(PropertyName = "permission")]
        public Permission Permission { get; set; }

    }
}
