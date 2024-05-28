using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Validation.Resources
{
    /// <summary>
    /// Validator for the <see cref="VectorizationRequest"/> model.
    /// </summary>
    public class VectorizationRequestValidator : AbstractValidator<VectorizationRequest>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="VectorizationRequest"/> model.
        /// </summary>
        public VectorizationRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("The vectorization request Name field is required.");
                        
            // Optionally validate ObjectId if there are specific criteria it needs to meet, such as format.
            RuleFor(x => x.ObjectId)
                .NotEmpty().When(x => x.ObjectId != null)
                .WithMessage("The object ID must not be empty if provided.");

            RuleFor(x => x.ContentIdentifier)
                .Must(contentIdentifier =>
                    (!string.IsNullOrWhiteSpace(contentIdentifier?.UniqueId) ||
                    !string.IsNullOrWhiteSpace(contentIdentifier?.CanonicalId)) &&
                    contentIdentifier != null)
                .WithMessage("The vectorization request content identifier is invalid.");
           
            RuleFor(x => x.Steps)
                .Must(steps => steps.Select(step => step.Id).Distinct().Count() == steps.Count)
                .When(steps => steps != null) // This condition ensures that we don't evaluate the Must if Steps is null
                .WithMessage("The list of vectorization steps must contain unique names.");
        }
    }

    /// <summary>
    /// Validator for the creation of a <see cref="VectorizationRequest"/>.
    /// </summary>
    public class VectorizationRequestCreationTimeValidator : AbstractValidator<VectorizationRequest>
    {
        /// <summary>
        /// Configures the validation rules for the creation of a <see cref="VectorizationRequest"/>.
        /// </summary>
        public VectorizationRequestCreationTimeValidator()
        {
            Include(new VectorizationRequestValidator());

            // Rule to ensure the list of vectorization steps should not be empty  
            RuleFor(x => x.Steps)
                .NotEmpty()
                .WithMessage("The list of the vectorization steps should not be empty.");

            // Rule to ensure the completed steps of the vectorization request must be empty  
            RuleFor(x => x.CompletedSteps)
                .Must(completedSteps => completedSteps == null || completedSteps.Count == 0)
                .WithMessage("The completed steps of the vectorization request must be empty.");

            // Rule to ensure the list of remaining steps of the vectorization request should not be empty  
            RuleFor(x => x.RemainingSteps)
                .NotEmpty()
                .WithMessage("The list of the remaining steps of the vectorization request should not be empty.");
        }
    }
}
