namespace SimpleEventBus;

public interface IEventHandlerInvoker
{
    Task InvokeAsync(object @event);
}