namespace SimpleEventBus.RabbitMq;

/// <summary>
/// The delivery mode
/// </summary>
public struct DeliveryMode
{
    /// <summary>
    /// The non persistent
    /// </summary>
    public const int NonPersistent = 1;

    /// <summary>
    /// The persistent
    /// </summary>
    public const int Persistent = 2;
}