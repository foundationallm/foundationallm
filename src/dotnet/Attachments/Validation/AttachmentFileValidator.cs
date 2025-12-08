using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Attachment.Validation
{
    /// <summary>
    /// Validator for the <see cref="AttachmentFile"/> model.
    /// </summary>
    public class AttachmentFileValidator : AbstractValidator<AttachmentFile>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="AttachmentFile"/> model.
        /// </summary>
        public AttachmentFileValidator()
        {
            Include(new ResourceBaseValidator());

            RuleFor(x => x.Content)
                .NotNull()
                .Must(content => content!.ToMemory().Length > 0)
                .WithMessage("The attachment content is null.");
        }
    }
}
