using Microsoft.AspNetCore.Http;

namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an exception that maps to a HTTP status code.
    /// </summary>
    public class HttpStatusCodeException : Exception
    {
        /// <summary>
        /// Provides the HTTP status code associated with the exception.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeException"/> class with a default message.
        /// </summary>
        public HttpStatusCodeException() : this(null, StatusCodes.Status500InternalServerError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="statusCode">The HTTP status code associated with the exception.</param>
        public HttpStatusCodeException(string? message, int statusCode = StatusCodes.Status500InternalServerError) :
            base(message) => StatusCode = statusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStatusCodeException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="statusCode">The HTTP status code associated with the exception.</param>
        public HttpStatusCodeException(string? message, Exception? innerException, int statusCode = StatusCodes.Status500InternalServerError) :
            base(message, innerException) => StatusCode = statusCode;
    }
}
