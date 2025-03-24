using FluentValidation;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides validation for the <see cref="CodeSessionFileUploadRequest"/> model.
    /// </summary>
    public class CodeSessionFileUploadRequestValidator: AbstractValidator<CodeSessionFileUploadRequest>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="CodeSessionFileUploadRequest"/> model.
        /// </summary>
        public CodeSessionFileUploadRequestValidator()
        {
            RuleFor(request => request.FileNames)
                .NotEmpty()
                .WithMessage("The list of file names must be provided.");

            RuleForEach(request => request.FileNames)
                .NotEmpty()
                .WithMessage("None of the file names can be null or whitespace.");
        }
    }
}
