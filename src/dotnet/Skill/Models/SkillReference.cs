using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Skill;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Skill.Models
{
    /// <summary>
    /// Reference to a skill resource.
    /// </summary>
    public class SkillReference : ResourceReference
    {
        /// <summary>
        /// The object type of the skill.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                SkillTypes.Skill => typeof(Common.Models.ResourceProviders.Skill.Skill),
                _ => throw new ResourceProviderException($"The skill type {Type} is not supported.")
            };
    }
}
