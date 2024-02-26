using FluentValidation;
using FoundationaLLM.Common.Models.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Validation.Metadata
{
    /// <summary>
    /// Validator for the <see cref="LanguageModel"/> model.
    /// </summary>
    public class LanguageModelValidator : AbstractValidator<LanguageModel>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="LanguageModel"/> model.
        /// </summary>
        public LanguageModelValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.");

            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is required.");

            RuleFor(x => x.Temperature)
                .InclusiveBetween(0, 1).WithMessage("Temperature must be between 0 and 1.");

            RuleFor(x => x.ApiEndpoint)
                .NotEmpty().WithMessage("API Endpoint is required.");

            RuleFor(x => x.ApiKey)
                .NotEmpty().When(x => !string.IsNullOrEmpty(x.ApiEndpoint))
                .WithMessage("API Key is required when an API Endpoint is provided.");

            RuleFor(x => x.ApiVersion)
                .NotEmpty().When(x => !string.IsNullOrEmpty(x.ApiEndpoint))
                .WithMessage("API Version is required when an API Endpoint is provided.");
        }
    }
}
