using FluentValidation;
using FoundationaLLM.Authorization.Models;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Authorization.Validation
{
    /// <summary>
    /// Validator for the <see cref="ActionAuthorizationRequest"/> model.
    /// </summary>
    public class ActionAuthorizationRequestValidator : AbstractValidator<ActionAuthorizationRequest>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="ActionAuthorizationRequest"/> model.
        /// </summary>
        public ActionAuthorizationRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Action)
                .NotNull()
                .NotEmpty()
                .WithMessage("The action must be a valid string.")
                .Must(x => AuthorizableActions.Actions.ContainsKey(x))
                .WithMessage("The action must be a valid action.");

            RuleFor(x => x.PrincipalId)
                .NotNull()
                .NotEmpty()
                .WithMessage("The principal identifier must be a valid string.")
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("The principal identifier must be a valid GUID.");

            RuleForEach(x => x.SecurityGroupIds)
                .NotNull()
                .NotEmpty()
                .WithMessage("The security group identifier must be a valid string.")
                .Must(x => Guid.TryParse(x, out _))
                .WithMessage("The security group identifier must be a valid GUID.");

            RuleForEach(x => x.ResourcePaths)
                .NotNull()
                .NotEmpty()
                .WithMessage("The resource path must be a valid string.");
        }
    }
}
