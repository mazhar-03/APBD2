namespace DeviceManager.Entities;

/// <summary>
///     Used by devices that need to notify about power or battery status.
/// </summary>
public interface IPowerNotifier
{
    /// <summary>
    ///     Sends a battery or power-related notification message.
    /// </summary>
    /// <param name="notification">The message to show or log.</param>
    void Notify(string notification);
}