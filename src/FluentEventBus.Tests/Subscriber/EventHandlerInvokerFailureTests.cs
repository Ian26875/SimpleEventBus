using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using FluentEventBus.DependencyInjection;
using FluentEventBus.Event;
using FluentEventBus.ExceptionHandlers;
using FluentEventBus.Profile;
using FluentEventBus.Naming;
using FluentEventBus.Subscriber;

namespace FluentEventBus.Tests.Subscriber;

public class EventHandlerInvokerFailureTests
{
    [Fact(DisplayName = "InvokeAsync_HandlerThrowsWithoutErrorHandler_ShouldRethrow")]
    public async Task InvokeAsync_HandlerThrowsWithoutErrorHandler_ShouldRethrow()
    {
        var invoker = BuildInvoker<ProfileWithoutErrorHandler>();

        var act = () => invoker.InvokeAsync(new FailingEvent(), new Headers());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("handler failed");
    }

    [Fact(DisplayName = "InvokeAsync_ErrorHandlerSetsHandled_ShouldSwallow")]
    public async Task InvokeAsync_ErrorHandlerSetsHandled_ShouldSwallow()
    {
        var invoker = BuildInvoker<ProfileWithHandlingErrorHandler>();

        var act = () => invoker.InvokeAsync(new FailingEvent(), new Headers());

        await act.Should().NotThrowAsync();
        HandlingErrorHandler.Invoked.Should().BeTrue();
    }

    [Fact(DisplayName = "InvokeAsync_ErrorHandlerDoesNotSetHandled_ShouldRethrow")]
    public async Task InvokeAsync_ErrorHandlerDoesNotSetHandled_ShouldRethrow()
    {
        var invoker = BuildInvoker<ProfileWithObservingErrorHandler>();

        var act = () => invoker.InvokeAsync(new FailingEvent(), new Headers());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("handler failed");
        ObservingErrorHandler.Invoked.Should().BeTrue();
    }

    private static IEventHandlerInvoker BuildInvoker<TProfile>() where TProfile : EventProfile
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEventBus(builder =>
        {
            builder.WithProfile<TProfile>();
        });
        services.AddSingleton<FailingHandler>();
        services.AddSingleton<HandlingErrorHandler>();
        services.AddSingleton<ObservingErrorHandler>();
        services.AddSingleton<IEventNameRegistry>(new StubEventNameRegistry());

        var provider = services.BuildServiceProvider();
        ((EventProfileManager)provider.GetRequiredService<IEventProfileManager>()).Initialize();

        return provider.GetRequiredService<IEventHandlerInvoker>();
    }

    private sealed record FailingEvent;

    private sealed class FailingHandler : IEventHandler<FailingEvent>
    {
        public Task HandleAsync(FailingEvent @event, Headers headers, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("handler failed");
        }
    }

    private sealed class HandlingErrorHandler : IEventExceptionHandler
    {
        public static bool Invoked;

        public Task OnExceptionAsync(ExceptionContext context, CancellationToken cancellationToken)
        {
            Invoked = true;
            context.Handled = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ObservingErrorHandler : IEventExceptionHandler
    {
        public static bool Invoked;

        public Task OnExceptionAsync(ExceptionContext context, CancellationToken cancellationToken)
        {
            Invoked = true;
            return Task.CompletedTask;
        }
    }

    private sealed class ProfileWithoutErrorHandler : EventProfile
    {
        public ProfileWithoutErrorHandler()
        {
            this.On<FailingEvent>().HandledBy<FailingHandler>();
        }
    }

    private sealed class ProfileWithHandlingErrorHandler : EventProfile
    {
        public ProfileWithHandlingErrorHandler()
        {
            this.On<FailingEvent>().HandledBy<FailingHandler>()
                .OnError<HandlingErrorHandler>();
        }
    }

    private sealed class ProfileWithObservingErrorHandler : EventProfile
    {
        public ProfileWithObservingErrorHandler()
        {
            this.On<FailingEvent>().HandledBy<FailingHandler>()
                .OnError<ObservingErrorHandler>();
        }
    }

    private sealed class StubEventNameRegistry : IEventNameRegistry
    {
        private readonly Dictionary<string, Type> _types = new();

        public void Register(Type eventType) => _types[eventType.Name] = eventType;

        public string GetEventName(Type type) => type.Name;

        public Type GetEventType(string type) => _types[type];
    }
}
