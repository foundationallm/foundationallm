using FluentValidation;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.DataPipeline.Validation
{
    /// <summary>
    /// Validator for the <see cref="DataPipelineRun"/> model.
    /// </summary>
    public class DataPipelineRunValidator : AbstractValidator<DataPipelineRun>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="DataPipelineDefinition"/> model.
        /// </summary>
        public DataPipelineRunValidator()
        {
            Include(new AzureCosmosDBResourceValidator());

            RuleFor(dpr => dpr.TriggeringUPN)
                .NotEmpty()
                .WithMessage("The triggering user principal name is required for the data pipeline run.");

            RuleFor(dpr => dpr.CanonicalRunId)
                .NotEmpty()
                .WithMessage("The canonical run identifier is required for the data pipeline run.");

            RuleFor(dpr => dpr.TriggerName)
                .NotEmpty()
                .WithMessage("The trigger name is required for the data pipeline run.");

            RuleFor(dpr => dpr.DataPipelineObjectId)
                .NotEmpty()
                .Must(ValidationUtils.ValidateObjectId)
                .WithMessage("The data pipeline object identifier is required for the data pipeline run and it must be a valid FoundationaLLM object identifier.");

            RuleFor(dpr => dpr.Processor)
                .NotEmpty()
                .Must(p => DataPipelineRunProcessors.All.Contains(p))
                .WithMessage($"The processor name is required for the data pipeline run and it must be one of the following values: {string.Join(", ", DataPipelineRunProcessors.All)}.");
        }
    }
}
