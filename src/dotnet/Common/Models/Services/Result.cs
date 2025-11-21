using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Services
{
    /// <summary>
    /// Represents the outcome of an operation, indicating either success or failure and providing error information if
    /// applicable.
    /// </summary>
    /// <remarks>Use the Result class to encapsulate the success or failure state of an operation without
    /// relying on exceptions for control flow. When a failure occurs, the associated error can be accessed through the
    /// Error property. For results that carry a value on success, use the generic <see cref="Result{T}"/> type.</remarks>
    public class Result
    {
        private Result(bool isSuccess, DomainError? error)
        {
            IsSuccess = isSuccess;
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
                ? new OkResult()
                : new ObjectResult(Error);

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A <see cref="Result"/> object indicating success.</returns>
        public static Result Success() =>
            new(true, null);

        /// <summary>
        /// Creates a failed result with the specified domain error.
        /// </summary>
        /// <param name="error">The domain error that describes the reason for the failure. Cannot be null.</param>
        /// <returns>A <see cref="Result"/> instance representing a failure with the provided domain error.</returns>
        public static Result Failure(DomainError error) =>
            error is null
                ? throw new ArgumentNullException(nameof(error))
                : new(false, error);

        /// <summary>
        /// Creates a failed result based on the error information extracted from the specified HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response message from which to extract error details. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a failed result populated with
        /// error information from the HTTP response.</returns>
        public static async Task<Result> FailureFromHttpResponse(HttpResponseMessage response)
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
        public static Result FailureFromErrorMessage(
            string errorMessage,
            int status = 500,
            string? instance = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(errorMessage);
            var error = DomainError.FromErrorMessage(errorMessage, status: status, instance: instance);
            return Failure(error);
        }

        /// <summary>
        /// Creates a failed result that represents the specified exception.
        /// </summary>
        /// <param name="exception">The exception to convert into a failure result. Cannot be null.</param>
        /// <param name="status">The status code associated with the exception.</param>
        /// <param name="instance">An optional instance identifier for the error.</param>
        /// <returns>A failed result containing error information derived from the specified exception.</returns>
        public static Result FailureFromException(
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
