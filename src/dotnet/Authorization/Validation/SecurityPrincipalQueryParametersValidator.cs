using FluentValidation;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Models.Authorization;

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
                    bool hasIds = x.Ids != null && x.Ids.Length > 0;

                    // Only one of the two options can be true
                    return (hasNameAndType && !hasIds) || (hasIds && string.IsNullOrWhiteSpace(x.Name) && string.IsNullOrWhiteSpace(x.SecurityPrincipalType));
                })
                .WithMessage("Specify either both Name and SecurityPrincipalType, or Ids, but not both.");


            RuleFor(x => x.SecurityPrincipalType)
                .Must(type => string.IsNullOrWhiteSpace(type) || SecurityPrincipalTypes.All.Contains(type))
                .WithMessage("SecurityPrincipalType must be one of the allowed values.");
        }
    }
}
