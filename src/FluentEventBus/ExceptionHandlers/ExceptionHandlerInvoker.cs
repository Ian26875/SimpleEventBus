using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentEventBus.Profile;

namespace FluentEventBus.ExceptionHandlers;

/// <summary>
///     The exception handler pipeline class
/// </summary>
/// <seealso cref="IExceptionHandlerInvoker" />
public class ExceptionHandlerInvoker : IExceptionHandlerInvoker
{
    /// <summary>
    ///     The service scope factory
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    ///     The subscription profile manager
    /// </summary>
    private readonly IEventProfileManager _eventProfileManager;

    /// <summary>
    ///     The logger
    /// </summary>
    private readonly ILogger<ExceptionHandlerInvoker> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ExceptionHandlerInvoker" /> class
    /// </summary>
    /// <param name="eventProfileManager">The subscription profile manager</param>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <param name="logger">The logger</param>
    public ExceptionHandlerInvoker(IEventProfileManager eventProfileManager,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ExceptionHandlerInvoker> logger)
    {
        _eventProfileManager = eventProfileManager;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    /// <summary>
    ///     Executes the context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task ExecuteAsync(ExceptionContext context, CancellationToken cancellationToken)
    {
        var errorHandlerTypes = _eventProfileManager.GetErrorHandlersForEvent(context.Event.GetType());
        if (errorHandlerTypes.Count == 0)
        {
            _logger.LogError(context.Exception,
                "Unhandled exception while processing event {EventType} and no error handler is registered. " +
                "The exception will propagate to the transport.", context.Event.GetType().Name);
            return;
        }

        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

        var serviceProvider = serviceScope.ServiceProvider;

        foreach (var exceptionHandler in errorHandlerTypes.Select(errorHandlerType => (IEventExceptionHandler) serviceProvider.GetRequiredService(errorHandlerType)))
        {
            await exceptionHandler.OnExceptionAsync(context, cancellationToken);
        }
    }
}
