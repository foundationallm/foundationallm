using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FoundationaLLM.Common.Models.Services
{
    /// <summary>
    /// Represents the result of an operation that can succeed with a value of a specified reference type or fail with a
    /// domain error.
    /// </summary>
    /// <typeparam name="T">The type of the value returned when the operation is successful. Must be a reference type.</typeparam>
    public class Result<T>
        where T : class
    {
        private Result(bool isSuccess, T? value, DomainError? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the result refers to a successful outcome.
        /// </summary>
        [JsonPropertyName("is_success")]
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the result represents a failure state.
        /// </summary>
        [JsonIgnore]
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error associated with a failure result.
        /// </summary>
        [JsonPropertyName("error")]
        public DomainError? Error { get; }

        /// <summary>
        /// Gets the value associated with the result.
        /// </summary>
        [JsonPropertyName("value")]
        public T? Value { get; }

        /// <summary>
        /// Attempts to retrieve the stored value if the operation was successful.
        /// </summary>
        /// <param name="value">When this method returns, contains the value associated with a successful result; otherwise, the default
        /// value for the type <typeparamref name="T"/>.</param>
        /// <returns>true if the operation was successful and a value is available; otherwise, false.</returns>
        public bool TryGetValue(out T value)
        {
            value = Value!;
            return IsSuccess;
        }

        /// <summary>
        /// Converts the current result to an appropriate <see cref="ActionResult"/> for use in ASP.NET Core MVC
        /// controllers.
        /// </summary>
        /// <remarks>Use this method to translate the result of an operation into a standardized HTTP
        /// response for controller actions. The returned result can be returned directly from an action
        /// method.</remarks>
        /// <returns>An <see cref="OkResult"/> if the operation was successful; otherwise, an <see cref="ObjectResult"/>
        /// containing the <see cref="ProblemDetails"/> object with error information.</returns>
        public ActionResult ToActionResult() =>
            IsSuccess
                ? new OkObjectResult(Value)
                : new ObjectResult(Error);

        /// <summary>
        /// Creates a successful result containing the specified value.
        /// </summary>
        /// <param name="value">The value to be encapsulated in the successful result.</param>
        /// <returns>A new <see cref="Result{T}"/> instance representing a successful outcome with the specified value.</returns>
        public static Result<T> Success(T value) =>
            value is null
                ? throw new ArgumentNullException(nameof(value))
                : new(true, value, null);

        /// <summary>
        /// Creates a failed result with the specified domain error.
        /// </summary>
        /// <param name="error">The domain error that describes the reason for the failure. Cannot be null.</param>
        /// <returns>A failed result containing the specified domain error.</returns>
        public static Result<T> Failure(DomainError error) =>
            error is null
                ? throw new ArgumentNullException(nameof(error))
                : new(false, null, error);

        /// <summary>
        /// Creates a failed result based on the specified HTTP response message.
        /// </summary>
        /// <param name="response">The HTTP response message from which to extract error information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a failed result with error
        /// details extracted from the HTTP response.</returns>
        public static async Task<Result<T>> FailureFromHttpResponse(HttpResponseMessage response)
        {
            ArgumentNullException.ThrowIfNull(response);
            var error = await DomainError.FromHttpResponse(response);
            return Failure(error);
        }

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message that describes the reason for the failure. Cannot be null or empty.</param>
        /// <param name="status">The status code associated with the exception.</param>
        /// <param name="instance">An optional instance identifier for the error.</param>
        /// <returns>A failed result containing the specified error message.</returns>
        public static Result<T> FailureFromErrorMessage(
            string errorMessage,
            int status = 500,
            string? instance = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(errorMessage);
            var error = DomainError.FromErrorMessage(errorMessage, status: status, instance: instance);
            return Failure(error);
        }

        /// <summary>
        /// Creates a failed result from the specified exception.
        /// </summary>
        /// <param name="exception">The exception to convert into a failure result. Cannot be null.</param>
        /// <param name="status">The status code associated with the exception.</param>
        /// <param name="instance">An optional instance identifier for the error.</param>
        /// <returns>A failed result containing error information derived from the specified exception.</returns>
        public static Result<T> FailureFromException(
            Exception exception,
            int status = 500,
            string? instance = null)
        {
            ArgumentNullException.ThrowIfNull(exception);
            var error = DomainError.FromException(exception, status: status, instance: instance);
            return Failure(error);
        }
    }
}
