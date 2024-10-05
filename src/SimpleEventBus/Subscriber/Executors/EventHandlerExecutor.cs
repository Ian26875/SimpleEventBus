using System.Linq.Expressions;
using System.Reflection;
using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber.Executors;

/// <summary>
/// The event handler executor class
/// </summary>
public class EventHandlerExecutor
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
    /// Initializes a new instance of the <see cref="EventHandlerExecutor"/> class
    /// </summary>
    /// <param name="handlerType">The handler type</param>
    /// <param name="eventType">The event type</param>
    /// <param name="methodInfo">The method info</param>
    public EventHandlerExecutor(Type handlerType, Type eventType,MethodInfo methodInfo)
    {
        HandlerType = handlerType;
        EventType = eventType;
        MethodInfo = methodInfo;
    }
    
    /// <summary>
    /// Creates the handler delegate
    /// </summary>
    /// <returns>A func of object and object and headers and cancellation token and task</returns>
    public Func<object, object, Headers, CancellationToken, Task> CreateHandlerDelegate()
    {
        return CreateDelegate(HandlerType, EventType, MethodInfo);
    }

    /// <summary>
    /// Gets the method info using the specified handler type
    /// </summary>
    /// <param name="handlerType">The handler type</param>
    /// <param name="eventType">The event type</param>
    /// <exception cref="MissingMethodException">Handler {handlerType.Name} does not implement the event handler for event {eventType.Name}</exception>
    /// <returns>The method info</returns>
    private static MethodInfo GetMethodInfo(Type handlerType, Type eventType)
    {
        // 获取 IEventHandler<TEvent> 的方法
        var eventHandlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var methodInfo = handlerType.GetInterfaceMap(eventHandlerInterfaceType).TargetMethods.FirstOrDefault();
        if (methodInfo is null)
        {
            throw new MissingMethodException($"Handler {handlerType.Name} does not implement the event handler for event {eventType.Name}");
        }

        return methodInfo;
    }

    /// <summary>
    /// Creates the delegate using the specified handler type
    /// </summary>
    /// <param name="handlerType">The handler type</param>
    /// <param name="eventType">The event type</param>
    /// <param name="methodInfo">The method info</param>
    /// <returns>A func of object and object and headers and cancellation token and task</returns>
    private static Func<object, object, Headers, CancellationToken, Task> CreateDelegate(Type handlerType, Type eventType, MethodInfo methodInfo)
    {
        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var eventParam = Expression.Parameter(typeof(object), "event");
        var headersParam = Expression.Parameter(typeof(Headers), "headers");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var castHandler = Expression.Convert(handlerParam, handlerType);
        var castEvent = Expression.Convert(eventParam, eventType);

        var call = Expression.Call
        (
            castHandler,
            methodInfo,
            castEvent,
            headersParam,
            cancellationTokenParam
        );

        return Expression.Lambda<Func<object, object, Headers, CancellationToken, Task>>
        (
            call, 
            handlerParam, 
            eventParam, 
            headersParam,
            cancellationTokenParam
        ).Compile();
    }
}
