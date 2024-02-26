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
    /// Validator for the <see cref="IndexingProfile"/> model.
    /// </summary>
    public class IndexingProfileValidator : AbstractValidator<IndexingProfile>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="IndexingProfile"/> model.
        /// </summary>
        public IndexingProfileValidator()
        {
            // Include the base validator to apply its rules.
            Include(new VectorizationProfileBaseValidator());

            RuleFor(x => x.Indexer)
                .IsInEnum().WithMessage("The indexer type must be a valid value.");
        }
    }
}
