using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.DataSource.Validation
{
    /// <summary>
    /// Validator for the <see cref="AzureSQLDatabaseDataSource"/> model.
    /// </summary>
    public class AzureSQLDatabaseDataSourceValidator : DataSourceValidator<AzureSQLDatabaseDataSource>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="AzureSQLDatabaseDataSource"/> model.
        /// </summary>
        public AzureSQLDatabaseDataSourceValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            Include(new DataSourceBaseValidator());

            RuleFor(x => x.Tables)
                .NotNull()
                .NotEmpty()
                .WithMessage("The list of table names must contain at least one value.");

            RuleForEach(x => x.Tables)
                .NotNull()
                .NotEmpty()
                .WithMessage("Each table name must be a valid string.");

            RuleFor(x => x)
                .Must(ds => ValidConfigurationReference(ds, "ConnectionString"))
                .WithMessage("The ConnectionString configuration reference is missing or has an invalid value.");
        }
    }
}
