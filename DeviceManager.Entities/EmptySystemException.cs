namespace DeviceManager.Entities;

/// <summary>
///     Thrown when a computer tries to start without an operating system.
/// </summary>
public class EmptySystemException : Exception
{
    /// <summary>
    ///     Creates the exception with a message explaining the missing OS issue.
    /// </summary>
    /// <param name="message">The error message to show.</param>
    public EmptySystemException(string? message) : base(message)
    {
    }
}