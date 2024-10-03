using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.RabbitMq;

public class RabbitMqEventSubscriber : AbstractEventSubscriber
{
    public RabbitMqEventSubscriber(IEventHandlerInvoker eventHandlerInvoker, IEventExceptionHandler eventExceptionHandler, ISubscriptionProfileManager subscriptionProfileManager) : base(eventHandlerInvoker, eventExceptionHandler, subscriptionProfileManager)
    {
    }
}