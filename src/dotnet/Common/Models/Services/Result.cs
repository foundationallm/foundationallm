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
        /// <summary>
        /// Initializes a new instance of the Result class that represents a successful result.
        /// </summary>
        protected Result() => IsSuccess = true;

        /// <summary>
        /// Initializes a new instance of the Result class that represents a failed operation with the specified error.
        /// </summary>
        /// <param name="error">The error that describes the reason for the failure. Cannot be null.</param>
        protected Result(DomainError error)
        {
            IsSuccess = false;
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
        /// Creates a successful result.
        /// </summary>
        /// <returns>A <see cref="Result"/> object indicating success.</returns>
        public static Result Success() => new();

        /// <summary>
        /// Creates a failed result with the specified domain error.
        /// </summary>
        /// <param name="error">The domain error that describes the reason for the failure. Cannot be null.</param>
        /// <returns>A <see cref="Result"/> instance representing a failure with the provided domain error.</returns>
        public static Result Failure(DomainError error) => new(error);

        /// <summary>
        /// Creates a failed result based on the error information extracted from the specified HTTP response.
        /// </summary>
        /// <param name="response">The HTTP response message from which to extract error details. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a failed result populated with
        /// error information from the HTTP response.</returns>
        public static async Task<Result> FailureFromHttpResponse(HttpResponseMessage response)
        {
            var error = await DomainError.FromHttpResponse(response);
            return Failure(error);
        }
    }
}
