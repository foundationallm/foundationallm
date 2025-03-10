using FluentValidation;
using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.Common.Validation.Plugins
{
    /// <summary>
    /// Validator for the <see cref="PluginComponent"/> model.
    /// </summary>
    public class PluginComponentValidator: AbstractValidator<PluginComponent>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="PluginComponent"/> model.
        /// </summary>
        public PluginComponentValidator()
        {
        }
    }
}
