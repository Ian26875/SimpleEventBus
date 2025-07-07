using System.Linq.Expressions;
using System.Reflection;
using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber.Executors;

/// <summary>
/// A unified event handler executor that supports handler expressions with or without Headers parameter.
/// </summary>
/// <typeparam name="TEvent">The event type.</typeparam>
/// <typeparam name="THandler">The handler type.</typeparam>
public class LambdaEventHandlerExecutor<TEvent, THandler> : IEventHandlerExecutor
    where THandler : class
    where TEvent : class
{
    private readonly Func<object, object, Headers, CancellationToken, Task> _compiledInvoker;

    /// <summary>
    /// Initializes a new instance for handler expressions that include Headers.
    /// </summary>
    public LambdaEventHandlerExecutor(Expression<Func<THandler, Func<TEvent, Headers, CancellationToken, Task>>> handlerExpression)
    {
        HandlerType = typeof(THandler);
        EventType = typeof(TEvent);
        MethodInfo = GetMethodInfo(handlerExpression);
        _compiledInvoker = CompileInvokerWithHeaders(handlerExpression);
    }

    /// <summary>
    /// Initializes a new instance for handler expressions that do NOT require Headers.
    /// </summary>
    public LambdaEventHandlerExecutor(Expression<Func<THandler, Func<TEvent, CancellationToken, Task>>> handlerExpression)
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

    private static Func<object, object, Headers, CancellationToken, Task> CompileInvokerWithHeaders(
        Expression<Func<THandler, Func<TEvent, Headers, CancellationToken, Task>>> handlerExpression)
    {
        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var eventParam = Expression.Parameter(typeof(object), "event");
        var headersParam = Expression.Parameter(typeof(Headers), "headers");
        var tokenParam = Expression.Parameter(typeof(CancellationToken), "token");

        var castedHandler = Expression.Convert(handlerParam, typeof(THandler));
        var castedEvent = Expression.Convert(eventParam, typeof(TEvent));

        var invokeInner = Expression.Invoke(handlerExpression, castedHandler);
        var invokeHandler = Expression.Invoke(invokeInner, castedEvent, headersParam, tokenParam);

        return Expression.Lambda<Func<object, object, Headers, CancellationToken, Task>>(invokeHandler, handlerParam, eventParam, headersParam, tokenParam)
                         .Compile();
    }

    private static MethodInfo GetMethodInfo(Expression method)
    {
        // Ensure the expression is a LambdaExpression
        if (method is not LambdaExpression lambda)
        {
            throw new ArgumentException("The provided expression must be a valid lambda expression.");
        }

        Expression expressionBody = lambda.Body;

        // Unwrap unary conversions (e.g., cast to object)
        if (expressionBody is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        {
            expressionBody = unaryExpression.Operand;
        }

        // Validate the expression body is a method call
        if (expressionBody is not MethodCallExpression methodCall)
        {
            throw new ArgumentException("The lambda expression format is invalid. It should be in the form 'x => x.Method(...)'.");
        }

        // Attempt to extract MethodInfo from the method call expression
        if (methodCall.Object is ConstantExpression { Value: MethodInfo methodInfo })
        {
            return methodInfo;
        }

        throw new ArgumentException("Unable to extract MethodInfo from the lambda expression. Ensure it references a method directly.");
    }
}
