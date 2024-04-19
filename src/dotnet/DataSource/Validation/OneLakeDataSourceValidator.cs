using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.DataSource.Validation
{
    /// <summary>
    /// Validator for the <see cref="OneLakeDataSource"/> model.
    /// </summary>
    public class OneLakeDataSourceValidator : DataSourceValidator<OneLakeDataSource>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="OneLakeDataSource"/> model.
        /// </summary>
        public OneLakeDataSourceValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            Include(new DataSourceBaseValidator());

            RuleFor(x => x.Workspaces)
                .NotNull()
                .NotEmpty()
                .WithMessage("The list of workspace names must contain at least one value.");

            RuleForEach(x => x.Workspaces)
                .NotNull()
                .NotEmpty()
                .WithMessage("Each workspace name must be a valid string.");

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
