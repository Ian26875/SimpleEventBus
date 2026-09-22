using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using FluentEventBus.DependencyInjection;
using FluentEventBus.Event;
using FluentEventBus.ExceptionHandlers;
using FluentEventBus.Profile;
using FluentEventBus.Schema;
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

    private static IEventHandlerInvoker BuildInvoker<TProfile>() where TProfile : SubscriptionProfile
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
        services.AddSingleton<IEventMapper>(new StubEventMapper());

        var provider = services.BuildServiceProvider();
        ((SubscriptionProfileManager)provider.GetRequiredService<ISubscriptionProfileManager>()).Initialize();

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

    private sealed class ProfileWithoutErrorHandler : SubscriptionProfile
    {
        public ProfileWithoutErrorHandler()
        {
            this.WhenOccurs<FailingEvent>().ToDo<FailingHandler>();
        }
    }

    private sealed class ProfileWithHandlingErrorHandler : SubscriptionProfile
    {
        public ProfileWithHandlingErrorHandler()
        {
            this.WhenOccurs<FailingEvent>().ToDo<FailingHandler>()
                .CatchExceptionToDo<HandlingErrorHandler>();
        }
    }

    private sealed class ProfileWithObservingErrorHandler : SubscriptionProfile
    {
        public ProfileWithObservingErrorHandler()
        {
            this.WhenOccurs<FailingEvent>().ToDo<FailingHandler>()
                .CatchExceptionToDo<ObservingErrorHandler>();
        }
    }

    private sealed class StubEventMapper : IEventMapper
    {
        private readonly Dictionary<string, Type> _types = new();

        public void Register(Type eventType) => _types[eventType.Name] = eventType;

        public string GetEventName(Type type) => type.Name;

        public Type GetEventType(string type) => _types[type];
    }
}
