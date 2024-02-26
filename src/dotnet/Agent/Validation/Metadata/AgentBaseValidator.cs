using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Validation.Metadata;
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
        public AgentBaseValidator()
        {
            Include(new ResourceBaseValidator());

            //RuleFor(x => x.LanguageModel).NotNull().WithMessage("The agent's language model is required.");
            When(x => x.LanguageModel != null, () =>
            {
                RuleFor(x => x.LanguageModel)
                    .SetValidator(new LanguageModelValidator()!);
            });
            //RuleFor(x => x.ConversationHistory).NotNull().When(x => x.SessionsEnabled);
            RuleFor(x => x.Orchestrator).NotEmpty().WithMessage("The agent's orchestrator is required.");
        }
    }
}
