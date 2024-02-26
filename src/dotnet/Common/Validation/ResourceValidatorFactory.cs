using FluentValidation;
using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FoundationaLLM.Common.Validation
{
    /// <inheritdoc/>
    public class ResourceValidatorFactory(IServiceProvider serviceProvider) : IResourceValidatorFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        /// <inheritdoc/>
        public IValidator<T>? GetValidator<T>() where T : class =>
            _serviceProvider.GetService<IValidator<T>>();

        /// <inheritdoc/>
        public object? GetValidator(Type type)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(type);
            return _serviceProvider.GetService(validatorType);
        }
    }
}
