namespace Project;

/// <summary>
/// Thrown when there's a problem with a network name.
/// </summary>
public class ConnectionException : Exception
{
    /// <summary>
    /// Creates the exception with a custom error message.
    /// </summary>
    /// <param name="message">The error message to show.</param>
    public ConnectionException(string message) : base(message)
    {
    }
}
