namespace FoundationaLLM.Common.Tasks;

/// <summary>
/// Represents errors that occur in task pools.
/// </summary>
public class TaskPoolException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolException"/> class with a default message.
    /// </summary>
    public TaskPoolException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolException"/> class with its message set to <paramref name="message"/>.
    /// </summary>
    /// <param name="message">A string that describes the error.</param>
    public TaskPoolException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolException"/> class with its message set to <paramref name="message"/>.
    /// </summary>
    /// <param name="message">A string that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public TaskPoolException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
