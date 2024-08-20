using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;

namespace FoundationaLLM.Attachment.Validation
{
    /// <summary>
    /// Base validator for attachments.
    /// </summary>
    /// <typeparam name="T">The type of attachment to validate.</typeparam>
    public class AttachmentValidator<T> : AbstractValidator<T> where T : AttachmentFile
    {
        /// <summary>
        /// Validates the value of a specified configuration reference.
        /// </summary>
        /// <param name="attachment">The attachment object being validated.</param>
        /// <returns>True if the value of the configuration reference is valid, False otherwise.</returns>
        protected bool ValidConfigurationReference(AttachmentFile attachment) =>
            true;
    }
}
