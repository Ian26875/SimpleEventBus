using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
/// The event subscriber interface
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Registers the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The value task</returns>
    ValueTask RegisterAsync(CancellationToken cancellationToken);
    
}