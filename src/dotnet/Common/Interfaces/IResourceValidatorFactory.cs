using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides methods for getting validators.
    /// </summary>
    public interface IResourceValidatorFactory
    {
        /// <summary>
        /// Gets a validator for the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object being validated.</typeparam>
        /// <returns></returns>
        IValidator<T>? GetValidator<T>() where T : class;

        /// <summary>
        /// Gets a validator for the <see cref="Type"></see> parameter, providing
        /// a non-generic option to resolve validators at runtime.
        /// </summary>
        /// <param name="type">The type of object being validated.</param>
        /// <returns></returns>
        object? GetValidator(Type type);
    }
}
