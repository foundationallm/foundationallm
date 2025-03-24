using FluentValidation;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides validation for the <see cref="CreateCodeSessionRequest"/> model.
    /// </summary>
    public class CreateCodeSessionRequestValidator: AbstractValidator<CreateCodeSessionRequest>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="CreateCodeSessionRequest"/> model.
        /// </summary>
        public CreateCodeSessionRequestValidator()
        {
            RuleFor(request => request.AgentName)
                .NotEmpty()
                .WithMessage("The agent name must be provided.");
            RuleFor(request => request.ConversationId)
                .NotEmpty()
                .WithMessage("The conversation identifier must be provided.");
            RuleFor(request => request.Context)
                .NotEmpty()
                .WithMessage("The context must be provided.");
        }
    }
}
