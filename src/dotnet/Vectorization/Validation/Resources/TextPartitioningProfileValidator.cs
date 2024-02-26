using FluentValidation;
using FoundationaLLM.Vectorization.Models.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Validation.Resources
{
    /// <summary>
    /// Validator for the <see cref="TextPartitioningProfile"/> model.
    /// </summary>
    public class TextPartitioningProfileValidator : AbstractValidator<TextPartitioningProfile>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="TextPartitioningProfile"/> model.
        /// </summary>
        public TextPartitioningProfileValidator()
        {
            // Include the base validator to apply its rules.
            Include(new VectorizationProfileBaseValidator());

            RuleFor(x => x.TextSplitter)
                .IsInEnum().WithMessage("The text splitter type must be a valid value.");
        }
    }
}
