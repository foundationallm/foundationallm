using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Context.Models
{
    public class ContextReference : ResourceReference
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override Type ResourceType => 
            Type switch
            {
                ContextTypes.KnowledgeSource => typeof(KnowledgeUnit),
                ContextTypes.KnowledgeUnit => typeof(KnowledgeUnit),
                _ => throw new ResourceProviderException($"The context type {Type} is not supported.")
            };
    }
}
