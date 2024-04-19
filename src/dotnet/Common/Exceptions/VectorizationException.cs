namespace FoundationaLLM.Common.Exceptions;

/// <summary>
/// Represents errors that occur during the vectorization process.
/// </summary>
public class VectorizationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VectorizationException"/> class with a default message.
    /// </summary>
    public VectorizationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorizationException"/> class with its message set to <paramref name="message"/>.
    /// </summary>
    /// <param name="message">A string that describes the error.</param>
    public VectorizationException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorizationException"/> class with its message set to <paramref name="message"/>.
    /// </summary>
    /// <param name="message">A string that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public VectorizationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
