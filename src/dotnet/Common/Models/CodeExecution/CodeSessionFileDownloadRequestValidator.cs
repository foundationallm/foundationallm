using FluentValidation;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides validation for the <see cref="CodeSessionFileDownloadRequest"/> model.
    /// </summary>
    public class CodeSessionFileDownloadRequestValidator: AbstractValidator<CodeSessionFileDownloadRequest>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="CodeSessionFileDownloadRequest"/> model.
        /// </summary>
        public CodeSessionFileDownloadRequestValidator() =>
            RuleFor(request => request.OperationId)
                .NotEmpty()
                .WithMessage("The operation identifier must be provided.");
    }
}
