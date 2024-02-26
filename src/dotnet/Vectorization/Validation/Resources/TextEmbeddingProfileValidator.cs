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
    /// Validator for the <see cref="TextEmbeddingProfile"/> model.
    /// </summary>
    public class TextEmbeddingProfileValidator : AbstractValidator<TextEmbeddingProfile>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="TextEmbeddingProfile"/> model.
        /// </summary>
        public TextEmbeddingProfileValidator()
        {
            // Include the base validator to apply its rules.
            Include(new VectorizationProfileBaseValidator());

            RuleFor(x => x.TextEmbedding)
                .IsInEnum().WithMessage("The text embedding type must be a valid value.");
        }
    }
}
