using System.Linq.Expressions;
using System.Reflection;
using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber.Executors;

/// <summary>
/// The expression event handler executor class
/// </summary>
/// <seealso cref="IEventHandlerExecutor"/>
public class ExpressionEventHandlerExecutor<TEvent,THandler> : IEventHandlerExecutor where THandler : class 
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

            throw new ArgumentException("Handler or event type mismatch");
        };
    }

    private static MethodInfo GetMethodInfo(Expression method)
    {
        // 檢查是否為 Lambda 表達式
        if (method is not LambdaExpression lambda)
        {
            throw new ArgumentException("參數不是有效的 Lambda 表達式。");
        }

        // 取得表達式的主體部分
        Expression expressionBody = lambda.Body;
    
        // 處理類型轉換表達式（例如 (c => (object)c.Method)）
        if (expressionBody is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        {
            expressionBody = unaryExpression.Operand;
        }

        // 確認主體是否為方法呼叫表達式
        if (expressionBody is not MethodCallExpression methodCall)
        {
            throw new ArgumentException("該表達式的格式不正確 (應該是 c => c.Method)。");
        }

        // 從常數表達式中取得方法資訊
        if (methodCall.Object is ConstantExpression constantExpression && constantExpression.Value is MethodInfo methodInfo)
        {
            return methodInfo;
        }

        throw new ArgumentException("無法從表達式中取得方法資訊。");
    }


}