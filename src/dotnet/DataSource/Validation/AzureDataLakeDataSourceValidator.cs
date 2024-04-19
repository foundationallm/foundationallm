using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.DataSource.Validation
{
    /// <summary>
    /// Validator for the <see cref="AzureDataLakeDataSource"/> model.
    /// </summary>
    public class AzureDataLakeDataSourceValidator : DataSourceValidator<AzureDataLakeDataSource>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="AzureDataLakeDataSource"/> model.
        /// </summary>
        public AzureDataLakeDataSourceValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            Include(new DataSourceBaseValidator());

            RuleFor(x => x.Folders)
                .NotNull()
                .NotEmpty()
                .WithMessage("The list of folder paths must contain at least one value.");

            RuleForEach(x => x.Folders)
                .NotNull()
                .NotEmpty()
                .WithMessage("Each folder path must be a valid string.");

            RuleFor(x => x)
                .Must(ds => ValidConfigurationReference(ds, "AuthenticationType"))
                .WithMessage("The AuthenticationType configuration reference is missing or has an invalid value.");

            RuleFor(x => x)
                .Must(ds =>
                    ValidConfigurationReference(ds, "AccountName")
                    || ValidConfigurationReference(ds, "APIKey")
                    || ValidConfigurationReference(ds, "ConnectionString"))
                .WithMessage("The configuration references must contain a valid value for AccountName, APIKey, or ConnectionString.");
        }

        
    }
}
