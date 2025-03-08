using FluentValidation;
using FoundationaLLM.Common.Models.Plugins;

namespace FoundationaLLM.DataPipeline.Validation
{
    /// <summary>
    /// Validator for the <see cref="PluginComponent"/> model.
    /// </summary>
    public class PluginArtifactValidator: AbstractValidator<PluginComponent>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="PluginComponent"/> model.
        /// </summary>
        public PluginArtifactValidator()
        {
        }
    }
}
