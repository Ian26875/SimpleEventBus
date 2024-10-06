using System.Reflection;
using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber.Executors;

/// <summary>
/// The event handler executor interface
/// </summary>
public interface IEventHandlerExecutor
{
    /// <summary>
    /// Gets the value of the handler type
    /// </summary>
    public Type HandlerType { get; }
    
    /// <summary>
    /// Gets the value of the event type
    /// </summary>
    public Type EventType { get; }
    
    /// <summary>
    /// Gets the value of the method info
    /// </summary>
    public MethodInfo MethodInfo { get; }
    
    /// <summary>
    /// Creates the handler delegate
    /// </summary>
    /// <returns>A func of object and object and headers and cancellation token and task</returns>
    Func<object, object, Headers, CancellationToken, Task> CreateHandlerDelegate();
}