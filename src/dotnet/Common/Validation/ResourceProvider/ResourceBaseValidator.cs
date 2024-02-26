using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                .Matches("^[a-z0-9_-]+$").WithMessage("Resource name must be lowercase and contain only letters, numbers, hyphens, and underscores.");
            RuleFor(x => x.Type).NotEmpty().WithMessage("Resource type is required.");
            RuleFor(x => x.ObjectId).NotEmpty().WithMessage("Resource object ID is required.");
        }
    }
}
