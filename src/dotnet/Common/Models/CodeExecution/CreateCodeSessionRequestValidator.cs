using FluentValidation;
using FoundationaLLM.Common.Constants.Context;

namespace FoundationaLLM.Common.Models.CodeExecution
{
    /// <summary>
    /// Provides validation for the <see cref="CreateCodeSessionRequest"/> model.
    /// </summary>
    public class CreateCodeSessionRequestValidator : AbstractValidator<CreateCodeSessionRequest>
    {
        /// <summary>
        /// The key used to store the user principal name in the validation context.
        /// </summary>
        public const string UserPrincipalNameKey = "UserPrincipalName";

        /// <summary>
        /// Configures the validation rules for the <see cref="CreateCodeSessionRequest"/> model.
        /// </summary>
        public CreateCodeSessionRequestValidator()
        {
            RuleFor(request => request.AgentName)
                .NotEmpty()
                .WithMessage("The agent name must be provided.");

            RuleFor(request => request.ConversationId)
                .NotEmpty()
                .WithMessage("The conversation identifier must be provided.");

            RuleFor(request => request.Context)
                .NotEmpty()
                .WithMessage("The context must be provided.");

            RuleFor(request => request.EndpointProvider)
                .NotEmpty()
                .WithMessage("The endpoint provider must be provided.");
            RuleFor(request => request.Language)
                .NotEmpty()
                .WithMessage("The language must be provided.")
                .Must(lang => CodeSessionLanguages.All.Contains(lang))
                .WithMessage($"The language must be one of the following: {string.Join(", ", CodeSessionLanguages.All)}.");

            RuleFor(request => request.EndpointProviderOverride)
                .Custom((endpointProviderOverride, context) =>
                {
                    if (endpointProviderOverride is null)
                        return;

                    if (!endpointProviderOverride.Enabled)
                    {
                        context.AddFailure("The context provider override is not enabled.");
                        return;
                    }

                    // Validate that the override UPN matches the current user if provided in context.
                    if (context.RootContextData.TryGetValue(UserPrincipalNameKey, out var upnObj)
                        && upnObj is string currentUpn
                        && !string.Equals(endpointProviderOverride.UPN, currentUpn, StringComparison.OrdinalIgnoreCase))
                    {
                        context.AddFailure(
                            nameof(CreateCodeSessionRequest.EndpointProviderOverride),
                            $"The endpoint provider override UPN '{endpointProviderOverride.UPN}' does not match the current user.");
                    }

                    if (string.IsNullOrWhiteSpace(endpointProviderOverride.Endpoint))
                    {
                        context.AddFailure(
                            nameof(CreateCodeSessionRequest.EndpointProviderOverride),
                            "The endpoint provider override endpoint must be provided when the override is enabled.");
                    }
                });
        }
    }
}
