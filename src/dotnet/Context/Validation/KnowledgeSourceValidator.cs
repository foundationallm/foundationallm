using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Context.Validation
{
    /// <summary>
    /// Provides validation methods for knowledge source objects.
    /// </summary>
    public class KnowledgeSourceValidator : AbstractValidator<KnowledgeSource>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="KnowledgeSource"/> model.
        /// </summary>
        public KnowledgeSourceValidator()
        {
            Include(new ResourceBaseValidator());

            RuleFor(ks => ks.KnowledgeUnitObjectIds)
                .NotEmpty()
                .WithMessage("At least one knowledge unit object identifier is required.");

            RuleForEach(ks => ks.KnowledgeUnitObjectIds)
                .NotEmpty()
                .Must(ValidationUtils.ValidateObjectId)
                .WithMessage("Each knowledge unit object identifier must be a valid FoundationaLLM object identifier.");
        }
    }
}
