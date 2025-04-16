using FluentValidation;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

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
            RuleFor(dpr => dpr.UPN)
                .NotEmpty()
                .WithMessage("The user principal name is required for the data pipeline run.");

            RuleFor(dpr => dpr.TriggeringUPN)
                .NotEmpty()
                .WithMessage("The triggering user principal name is required for the data pipeline run.");

            RuleFor(dpr => dpr.InstanceId)
                .Must(ValidateGuid)
                .WithMessage("The FoundationaLLM instance identifier must be a valid GUID.");

            RuleFor(dpr => dpr.TriggerName)
                .NotEmpty()
                .WithMessage("The trigger name is required for the data pipeline run.");

            RuleFor(dpr => dpr.DataPipelineObjectId)
                .NotEmpty()
                .Must(ValidateObjectId)
                .WithMessage("The data pipeline object identifier is required for the data pipeline run and it must be a valid FoundationaLLM object identifier.");
        }

        private bool ValidateGuid(string guid) => Guid.TryParse(guid, out _);

        private bool ValidateObjectId(string objectId) =>
            ResourcePath.TryParse(
                objectId,
                ResourceProviderNames.All,
                ResourceProviderMetadata.AllAllowedResourceTypes,
                true,
                out _);
    }
}
