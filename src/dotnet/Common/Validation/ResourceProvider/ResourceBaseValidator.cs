using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Validation.ResourceProvider
{
    /// <summary>
    /// Validator for the <see cref="ResourceBase"/> model.
    /// </summary>
    public class ResourceBaseValidator : AbstractValidator<ResourceBase>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="ResourceBase"/> model.
        /// </summary>
        public ResourceBaseValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Resource name is required.");
            // Create a rule for the Name property to ensure it is lowercase and contains only letters, numbers, hyphens, and underscores.
            RuleFor(x => x.Name)
                .Matches("^[a-zA-Z]([a-zA-Z0-9_-]?)+$").WithMessage("Resource name must start with a letter and contain only letters, numbers, hyphens, or underscores.");
            RuleFor(x => x.Type).NotEmpty().WithMessage("Resource type is required.");
            RuleFor(x => x.ObjectId).NotEmpty().WithMessage("Resource object ID is required.");
        }
    }
}
