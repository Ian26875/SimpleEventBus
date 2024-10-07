using System.Linq.Expressions;
using System.Reflection;
using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber.Executors;

/// <summary>
/// The expression event handler executor class
/// </summary>
/// <seealso cref="IEventHandlerExecutor"/>
public class ExpressionEventHandlerExecutor<TEvent, THandler> : IEventHandlerExecutor where THandler : class 
                                                                                     where TEvent : class
{
    /// <summary>
    /// The handler delegate
    /// </summary>
    private readonly Func<THandler, Func<TEvent, Headers, CancellationToken, Task>> _handlerDelegate;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionEventHandlerExecutor{TEvent,THandler}"/> class
    /// </summary>
    /// <param name="handlerMethod">The handler method</param>
    public ExpressionEventHandlerExecutor(Expression<Func<THandler, Func<TEvent, Headers, CancellationToken, Task>>> handlerMethod)
    {
        _handlerDelegate = handlerMethod.Compile();
        HandlerType = typeof(THandler);
        EventType = typeof(TEvent);
        MethodInfo = GetMethodInfo(handlerMethod);
    }

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
    ///     Creates the handler delegate
    /// </summary>
    public Func<object, object, Headers, CancellationToken, Task> CreateHandlerDelegate()
    {
        return (handler, @event, headers, cancellationToken) =>
        {
            if (handler is THandler typedHandler && @event is TEvent typedEvent)
            {
                var handlerFunc = _handlerDelegate(typedHandler);
                return handlerFunc(typedEvent, headers, cancellationToken);
            }

            throw new ArgumentException("The handler or event type does not match.");
        };
    }

    private static MethodInfo GetMethodInfo(Expression method)
    {
        if (method is not LambdaExpression lambda)
        {
            throw new ArgumentException("The argument is not a valid Lambda expression.");
        }
        
        var expressionBody = lambda.Body;
        
        if (expressionBody is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
        {
            expressionBody = unaryExpression.Operand;
        }
        
        if (expressionBody is not MethodCallExpression methodCall)
        {
            throw new ArgumentException("The format of the expression is incorrect (should be c => c.Method).");
        }
        
        if (methodCall.Object is ConstantExpression { Value: MethodInfo methodInfo })
        {
            return methodInfo;
        }

        throw new ArgumentException("Unable to retrieve method information from the expression.");
    }
}
