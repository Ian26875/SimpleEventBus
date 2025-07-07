using System.Linq.Expressions;
using System.Reflection;
using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber.Executors;

/// <summary>
/// Represents a compiled delegate handler executor using expression tree.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
/// <typeparam name="THandler">The handler type.</typeparam>
public class CompiledEventHandlerExecutor<TEvent, THandler> : IEventHandlerExecutor
    where THandler : class
    where TEvent : class
{
    private readonly Func<object, object, Headers, CancellationToken, Task> _compiledInvoker;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CompiledEventHandlerExecutor{TEvent, THandler}"/> class without headers.
    /// </summary>
    /// <param name="handlerExpression">The handler expression that does not require headers.</param>
    public CompiledEventHandlerExecutor(Expression<Func<THandler, Func<TEvent, CancellationToken, Task>>> handlerExpression)
    {
        HandlerType = typeof(THandler);
        EventType = typeof(TEvent);
        MethodInfo = GetMethodInfo(handlerExpression);
        _compiledInvoker = (handler, @event, _, token) =>
        {
            var compiled = handlerExpression.Compile();
            return compiled((THandler)handler)((TEvent)@event, token);
        };
    }

    public Type HandlerType { get; }
    public Type EventType { get; }
    public MethodInfo MethodInfo { get; }

    public Func<object, object, Headers, CancellationToken, Task> CreateHandlerDelegate() => _compiledInvoker;

    private Func<object, object, Headers, CancellationToken, Task> CompileInvoker(LambdaExpression handlerExpression)
    {
        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var eventParam = Expression.Parameter(typeof(object), "event");
        var headersParam = Expression.Parameter(typeof(Headers), "headers");
        var tokenParam = Expression.Parameter(typeof(CancellationToken), "token");

        var castedHandler = Expression.Convert(handlerParam, typeof(THandler));
        var castedEvent = Expression.Convert(eventParam, typeof(TEvent));

        var invokeInner = Expression.Invoke(handlerExpression, castedHandler);
        var invokeHandler = Expression.Invoke(invokeInner, castedEvent, headersParam, tokenParam);

        return Expression.Lambda<Func<object, object, Headers, CancellationToken, Task>>(
            invokeHandler, handlerParam, eventParam, headersParam, tokenParam).Compile();
    }

    private static MethodInfo GetMethodInfo(Expression expression)
    {
        if (expression is not LambdaExpression lambda)
        {
            throw new ArgumentException("The provided expression must be a valid lambda expression, e.g., 'x => x.Method'.");
        }

        Expression body = lambda.Body;
        
        if (body is UnaryExpression {NodeType: ExpressionType.Convert} unary)
        {
            body = unary.Operand;
        }

        if (body is MethodCallExpression {Method: not null} methodCall)
        {
            return methodCall.Method;
        }
        
        var method = body.Type.GetMethod("HandleAsync");
        if (method is not null)
        {
            return method;
        }
        
        throw new ArgumentException(
            $"Unable to extract MethodInfo from the expression: '{expression}'. " +
            $"Ensure it is a direct method call like 'x => x.Method(...)'. Expression type: {body.GetType().Name}");
    }

}
