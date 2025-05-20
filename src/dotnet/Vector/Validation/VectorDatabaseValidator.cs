using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.Vector;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Plugin.Validation
{
    /// <summary>
    /// Validator for the <see cref="VectorDatabase"/> model.
    /// </summary>
    public class VectorDatabaseValidator: AbstractValidator<VectorDatabase>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="VectorDatabase"/> model.
        /// </summary>
        public VectorDatabaseValidator()
        {
            Include(new ResourceBaseValidator());

            RuleFor(vd => vd.DatabaseName)
                .NotEmpty()
                .WithMessage("The vector database name is required.");

            RuleFor(vd => vd.EmbeddingPropertyName)
                .NotEmpty()
                .WithMessage("The embedding property name is required.");

            RuleFor(vd => vd.ContentPropertyName)
                .NotEmpty()
                .WithMessage("The content property name is required.");

            RuleFor(vd => vd.VectorStoreIdPropertyName)
                .NotEmpty()
                .WithMessage("The vector store identifier property name is required.");

            RuleFor(vd => vd.APIEndpointConfigurationObjectId)
                .NotEmpty()
                .Must(ValidationUtils.ValidateObjectId)
                .WithMessage("The API endpoint configuration object identifier is required for the vector database and it must be a valid FoundationaLLM object identifier.");

        }
    }
}
