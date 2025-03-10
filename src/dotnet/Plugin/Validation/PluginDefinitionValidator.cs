using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Plugin.Validation
{
    /// <summary>
    /// Validator for the <see cref="PluginDefinition"/> model.
    /// </summary>
    public class PluginDefinitionValidator: AbstractValidator<PluginDefinition>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="PluginDefinition"/> model.
        /// </summary>
        public PluginDefinitionValidator() => Include(new ResourceBaseValidator());
    }
}
