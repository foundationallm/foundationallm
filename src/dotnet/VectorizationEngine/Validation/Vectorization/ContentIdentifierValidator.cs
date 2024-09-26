using FluentValidation;
using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Vectorization.Validation.Vectorization
{
    /// <summary>
    /// Validator for the <see cref="ContentIdentifier"/> model.
    /// </summary>
    public class ContentIdentifierValidator : AbstractValidator<ContentIdentifier>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="ContentIdentifier"/> model.
        /// </summary>
        public ContentIdentifierValidator()
        {
            // Optionally validate Object id if there are specific criteria it needs to meet, such as format.
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.DataSourceObjectId))
                .WithMessage("The data source object ID must not be empty.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.CanonicalId))
                .WithMessage("The canonical ID must not be empty.");

            RuleForEach(x => x.MultipartId)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("The multipart component must not be empty.");

            When(x => x.Metadata != null && x.Metadata.Count != 0, () =>
            {
                RuleForEach(x => x.Metadata)
                    .Must((profile, kw) => !string.IsNullOrEmpty(kw.Key))
                    .WithMessage("Metadata keys must not be empty.");
            });
        }
    }
}
