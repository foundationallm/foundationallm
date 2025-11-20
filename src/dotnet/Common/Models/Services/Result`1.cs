using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Services
{
    /// <summary>
    /// Represents the result of an operation that can succeed with a value of a specified reference type or fail with a
    /// domain error.
    /// </summary>
    /// <typeparam name="T">The type of the value returned when the operation is successful. Must be a reference type.</typeparam>
    public class Result<T> : Result
        where T : class
    {
        private Result(T value) : base() => Value = value;

        private Result(DomainError error) : base(error) { }

        /// <summary>
        /// Gets the value associated with the result.
        /// </summary>
        [JsonPropertyName("value")]
        public T? Value { get; }

        /// <summary>
        /// Creates a successful result containing the specified value.
        /// </summary>
        /// <param name="value">The value to be encapsulated in the successful result.</param>
        /// <returns>A new <see cref="Result{T}"/> instance representing a successful outcome with the specified value.</returns>
        public static Result<T> Success(T value) => new(value);

        /// <summary>
        /// Creates a failed result with the specified domain error.
        /// </summary>
        /// <param name="error">The domain error that describes the reason for the failure. Cannot be null.</param>
        /// <returns>A failed result containing the specified domain error.</returns>
        public static new Result<T> Failure(DomainError error) => new(error);

        /// <summary>
        /// Creates a failed result based on the specified HTTP response message.
        /// </summary>
        /// <param name="response">The HTTP response message from which to extract error information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a failed result with error
        /// details extracted from the HTTP response.</returns>
        public static async new Task<Result<T>> FailureFromHttpResponse(HttpResponseMessage response)
        {
            var error = await DomainError.FromHttpResponse(response);
            return Failure(error);
        }
    }
}
