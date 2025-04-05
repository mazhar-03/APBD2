namespace DeviceManager.Entities;

/// <summary>
///     Thrown when a device (like a smartwatch) tries to turn on with too little battery.
/// </summary>
public class EmptyBatteryException : Exception
{
    /// <summary>
    ///     Creates the exception with a message explaining the battery issue.
    /// </summary>
    /// <param name="message">The error message to show.</param>
    public EmptyBatteryException(string? message) : base(message)
    {
    }
}