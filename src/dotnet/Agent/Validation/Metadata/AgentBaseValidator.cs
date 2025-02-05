using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Agent.Validation.Metadata
{
    /// <summary>
    /// Validator for the <see cref="AgentBase"/> model.
    /// </summary>
    public class AgentBaseValidator : AbstractValidator<AgentBase>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="AgentBase"/> model.
        /// </summary>
        public AgentBaseValidator() =>
            Include(new ResourceBaseValidator());
        //RuleFor(x => x.OrchestrationSettings!.Orchestrator).NotEmpty().WithMessage("The agent's orchestrator is required.");
    }
}
