using System.Linq.Expressions;
using System.Reflection;
using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber.Executors;

/// <summary>
/// The interface event handler executor class
/// </summary>
/// <seealso cref="IEventHandlerExecutor"/>
public class InterfaceEventHandlerExecutor<TEvent,THandler> : IEventHandlerExecutor where TEvent : class
                                                where THandler : IEventHandler<TEvent>
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
    /// Initializes a new instance of the <see cref="InterfaceEventHandlerExecutor{TEvent,THandler}"/> class
    /// </summary>
    public InterfaceEventHandlerExecutor()
    {
      
        EventType = typeof(TEvent);
        HandlerType = typeof(THandler);
        MethodInfo = GetMethodInfo(EventType);
    }
    
    public Func<object, object, Headers, CancellationToken, Task> CreateHandlerDelegate()
    {
        return CreateDelegate(HandlerType, EventType, MethodInfo);
    }
    
    private static MethodInfo GetMethodInfo(Type eventType)
    {
        var eventHandlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(eventType);

        var methodInfo = eventHandlerInterfaceType.GetMethod(nameof(IEventHandler<object>.HandleAsync));
        if (methodInfo is null)
        {
            throw new MissingMemberException($"{eventHandlerInterfaceType} miss 'HandlerAsync' method.");
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