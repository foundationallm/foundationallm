using FluentValidation;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.Vectorization.Validation.Resources
{
    /// <summary>
    /// Validator for the <see cref="VectorizationPipeline"/> model.
    /// </summary>
    public class VectorizationPipelineValidator : AbstractValidator<VectorizationPipeline>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="VectorizationPipeline"/> model.
        /// </summary>
        public VectorizationPipelineValidator()
        {
            Include(new ResourceBaseValidator());

            RuleFor(x => x.Name).NotEmpty().WithMessage("The vectorization pipeline Name is required.");

            // Optionally validate ObjectId if there are specific criteria it needs to meet, such as format.
            RuleFor(x => x.ObjectId)
                .NotEmpty().When(x => x.ObjectId != null)
                .WithMessage("The object ID must not be empty if provided.");

            RuleFor(x => x.DataSourceObjectId)
                .NotEmpty().When(x => x.DataSourceObjectId != null)
                .WithMessage("The data source object ID must not be empty if provided.");

            RuleFor(x => x.TextPartitioningProfileObjectId)
                .NotEmpty().When(x => x.TextPartitioningProfileObjectId != null)
                .WithMessage("The text partitioning profile object ID must not be empty if provided.");

            RuleFor(x => x.TextEmbeddingProfileObjectId)
                .NotEmpty().When(x => x.TextEmbeddingProfileObjectId != null)
                .WithMessage("The text embedding profile object ID must not be empty if provided.");

            RuleFor(x => x.IndexingProfileObjectId)
                .NotEmpty().When(x => x.IndexingProfileObjectId != null)
                .WithMessage("The indexing profile object ID must not be empty if provided.");

            // Validate the trigger Type property to ensure it's a valid enum value.
            RuleFor(x => x.TriggerType)
                .IsInEnum().WithMessage("The trigger type must be a valid value.");

            // Validate the trigger cron schedule when trigger Type is Schedule.
            RuleFor(x => x.TriggerCronSchedule)
                .NotEmpty().When(x => x.TriggerType == VectorizationPipelineTriggerType.Schedule)
                .WithMessage("The trigger cron schedule must not be empty if trigger type is Schedule.");
        }
    }
}
