using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Agent.Validation.Metadata
{
    /// <summary>
    /// Validator for the <see cref="Workflow"/> model.
    /// </summary>
    public class WorkflowValidator : AbstractValidator<Workflow>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="Workflow"/> model.
        /// </summary>
        public WorkflowValidator() => Include(new ResourceBaseValidator());
    }
}
