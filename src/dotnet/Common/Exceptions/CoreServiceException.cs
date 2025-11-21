using Microsoft.AspNetCore.Http;

namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an error with accessing content.
    /// </summary>
    public class CoreServiceException : HttpStatusCodeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreServiceException"/> class with a default message.
        /// </summary>
        public CoreServiceException() : this(null, StatusCodes.Status500InternalServerError)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreServiceException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="statusCode">The HTTP status code associated with the exception.</param>
        public CoreServiceException(string? message, int statusCode = StatusCodes.Status500InternalServerError) :
            base(message, statusCode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreServiceException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        /// <param name="statusCode">The HTTP status code associated with the exception.</param>
        public CoreServiceException(string? message, Exception? innerException, int statusCode = StatusCodes.Status500InternalServerError) :
            base(message, innerException, statusCode)
        {
        }
    }
}
