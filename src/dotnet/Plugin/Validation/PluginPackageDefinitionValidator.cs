using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Plugin;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Plugin.Validation
{
    /// <summary>
    /// Validator for the <see cref="PluginPackageDefinition"/> model.
    /// </summary>
    public class PluginPackageDefinitionValidator: AbstractValidator<PluginPackageDefinition>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="PluginPackageDefinition"/> model.
        /// </summary>
        public PluginPackageDefinitionValidator() => Include(new ResourceBaseValidator());
    }
}
