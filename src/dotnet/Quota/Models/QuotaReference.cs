using FoundationaLLM.Common.Models.Quota;
using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Quota.Models
{
    /// <summary>
    /// Provides details about a quota definition reference.
    /// </summary>
    public class QuotaReference : ResourceReference
    {
        /// <summary>
        /// The object type of the quota.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType => typeof(QuotaDefinition);
    }
}
