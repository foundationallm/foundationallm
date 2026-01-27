using FluentValidation;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using Microsoft.Graph.Models;

namespace FoundationaLLM.Authorization.Validation
{
    /// <summary>
    /// Validator for the <see cref="SecurityPrincipalQueryParameters"/> model.
    /// </summary>
    public class SecurityPrincipalQueryParametersValidator : AbstractValidator<SecurityPrincipalQueryParameters>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="SecurityPrincipalQueryParameters"/> model.
        /// </summary>
        public SecurityPrincipalQueryParametersValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x)
                .Must(x =>
                {
                    bool hasNameAndType = !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.SecurityPrincipalType);
                    bool hasUPNAndType = !string.IsNullOrWhiteSpace(x.UPN) && !string.IsNullOrWhiteSpace(x.SecurityPrincipalType);
                    bool hasIds = x.Ids != null && x.Ids.Length > 0;

                    // Exactly one must be true
                    return new[] { hasNameAndType, hasUPNAndType, hasIds }.Count(b => b) == 1;
                })
                .WithMessage("Specify only one of Name + SecurityPrincipalType, UPN + SecurityPrincipalType, or Ids.");

            RuleFor(x => x.SecurityPrincipalType)
                .Equal(SecurityPrincipalTypes.User)
                .When(x => !string.IsNullOrWhiteSpace(x.UPN))
                .WithMessage("SecurityPrincipalType must be 'User' when UPN is specified.");

            RuleFor(x => x.SecurityPrincipalType)
                .Must(type => string.IsNullOrWhiteSpace(type) || SecurityPrincipalTypes.All.Contains(type))
                .WithMessage("SecurityPrincipalType must be one of the allowed values.");
        }
    }
}
