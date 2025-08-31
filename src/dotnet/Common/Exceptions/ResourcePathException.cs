namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an error related to a resource path.
    /// </summary>
    public class ResourcePathException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePathException"/> class with a default message.
        /// </summary>
        public ResourcePathException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePathException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        public ResourcePathException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePathException"/> class with its message set to <paramref name="message"/>.
        /// </summary>
        /// <param name="message">A string that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ResourcePathException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
