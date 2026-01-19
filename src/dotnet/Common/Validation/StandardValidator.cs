using FluentValidation;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Validation
{
    /// <summary>
    /// Provides the standard validation implementation.
    /// </summary>
    /// <param name="validatorFactory">The resource validator factory used to create validators.</param>
    /// <param name="exceptionBuilder">The function that builds the exception thrown when the object instance is not valid.</param>
    public class StandardValidator(
        IResourceValidatorFactory validatorFactory,
        Func<string, Exception> exceptionBuilder)
    {
        private readonly IResourceValidatorFactory _validatorFactory = validatorFactory;
        private readonly Func<string, Exception> _exceptionBuilder = exceptionBuilder;

        /// <summary>
        /// Validates the specified object instance and throws an exception if the instance is invalid.
        /// </summary>
        /// <typeparam name="T">The type of the object instance to validate.</typeparam>
        /// <param name="instance">The object instance being validated.</param>
        public async Task ValidateAndThrowAsync<T>(T instance) where T : class =>
            await ValidateAndThrowAsync(instance, null);

        /// <summary>
        /// Validates the specified object instance with additional context data and throws an exception if the instance is invalid.
        /// </summary>
        /// <typeparam name="T">The type of the object instance to validate.</typeparam>
        /// <param name="instance">The object instance being validated.</param>
        /// <param name="contextData">Optional dictionary of additional context data to pass to the validator.</param>
        public async Task ValidateAndThrowAsync<T>(T instance, Dictionary<string, object>? contextData) where T : class
        {
            bool isValid = true;
            string? errorMessage = null;

            try
            {
                var validator = _validatorFactory.GetValidator<T>();
                if (validator == null)
                {
                    isValid = false;
                    errorMessage = $"No validator found for type {typeof(T).Name}.";
                }
                else
                {
                    var validationContext = new ValidationContext<T>(instance);

                    if (contextData is not null)
                    {
                        foreach (var kvp in contextData)
                            validationContext.RootContextData[kvp.Key] = kvp.Value;
                    }

                    var validationResult = await validator.ValidateAsync(validationContext);
                    if (!validationResult.IsValid)
                    {
                        isValid = false;
                        errorMessage = $"Validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, validationResult.Errors.Select(e => e.ErrorMessage))}";
                    }
                }
            }
            catch (Exception ex)
            {
                throw _exceptionBuilder(ex.Message);
            }

            if (!isValid)
            {
                throw _exceptionBuilder(errorMessage!);
            }
        }
    }
}
