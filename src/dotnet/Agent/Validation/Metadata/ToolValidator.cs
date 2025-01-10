using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Agent.Validation.Metadata
{
    /// <summary>
    /// Validator for the <see cref="Tool"/> model.
    /// </summary>
    public class ToolValidator : AbstractValidator<Tool>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="Tool"/> model.
        /// </summary>
        public ToolValidator() => Include(new ResourceBaseValidator());
    }
}
