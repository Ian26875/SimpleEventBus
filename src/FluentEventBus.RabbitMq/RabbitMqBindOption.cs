using FluentEventBus.Naming;

namespace FluentEventBus.RabbitMq
{
    /// <summary>
    /// The rabbit mq binding option class
    /// </summary>
    public class RabbitMqBindingOption
    {
        /// <summary>
        /// Gets or sets the value of the exchange bindings
        /// </summary>
        public Dictionary<string, string> ExchangeBindings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the value of the queue bindings
        /// </summary>
        public Dictionary<string, string> QueueBindings { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets or sets the global exchange name
        /// </summary>
        public string GlobalExchange { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the global queue name
        /// </summary>
        public string GlobalQueue { get; set; } = string.Empty;

        /// <summary>
        /// Service name used to build default queue name when GlobalQueue is not set.
        /// </summary>
        public string ServiceName { get; set; } = "app";

        /// <summary>
        /// Environment name used to build default queue name when GlobalQueue is not set.
        /// </summary>
        public string EnvironmentName { get; set; } = "prod";

        /// <summary>
        /// Prefetch count for RabbitMQ consumers.
        /// </summary>
        public ushort PrefetchCount { get; set; } = 10;

        /// <summary>
        /// Gets or sets the per-event dead letter exchange bindings (event name -> exchange name).
        /// </summary>
        public Dictionary<string, string> DeadLetterExchangeBindings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the per-event dead letter queue bindings (event name -> queue name).
        /// </summary>
        public Dictionary<string, string> DeadLetterQueueBindings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Global dead letter exchange used when an event has no per-event dead letter binding.
        /// Empty means dead lettering is disabled.
        /// </summary>
        public string GlobalDeadLetterExchange { get; set; } = string.Empty;

        /// <summary>
        /// Global dead letter queue used when an event has no per-event dead letter binding.
        /// </summary>
        public string GlobalDeadLetterQueue { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of redeliveries for a failed message before it is
        /// nacked without requeue (dead-lettered when a dead letter exchange is configured).
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;
        
        /// <summary>
        /// Gets or sets the value of the schema registry
        /// </summary>
        public IEventNameRegistry EventNameRegistry { get; set; } = null!;
    }

    /// <summary>
    /// The event binder class
    /// </summary>
    public class EventBinder<TEvent> where TEvent : class
    {
        /// <summary>
        /// The options
        /// </summary>
        private readonly RabbitMqBindingOption _options;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="EventBinder{TEvent}"/> class
        /// </summary>
        /// <param name="options">The options</param>
        public EventBinder(RabbitMqBindingOption options)
        {
            _options = options;
        }

        /// <summary>
        /// Declares the exchange using the specified exchange name
        /// </summary>
        /// <param name="exchangeName">The exchange name</param>
        /// <returns>An event binder of t event</returns>
        public EventBinder<TEvent> DeclareExchange(string exchangeName)
        {
            var eventName = _options.EventNameRegistry.GetEventName(typeof(TEvent));
            _options.ExchangeBindings[eventName] = exchangeName;
            return this;
        }

        /// <summary>
        /// Declares the queue using the specified queue name
        /// </summary>
        /// <param name="queueName">The queue name</param>
        /// <returns>An event binder of t event</returns>
        public EventBinder<TEvent> DeclareQueue(string queueName)
        {
            var eventName = _options.EventNameRegistry.GetEventName(typeof(TEvent));
            _options.QueueBindings[eventName] = queueName;
            return this;
        }

        /// <summary>
        /// Declares a dead letter exchange and queue for this event.
        /// Failed messages exceeding <see cref="RabbitMqBindingOption.MaxRetryCount"/> are routed there.
        /// </summary>
        /// <param name="exchangeName">The dead letter exchange name</param>
        /// <param name="queueName">The dead letter queue name</param>
        /// <returns>An event binder of t event</returns>
        public EventBinder<TEvent> WithDeadLetter(string exchangeName, string queueName)
        {
            var eventName = _options.EventNameRegistry.GetEventName(typeof(TEvent));
            _options.DeadLetterExchangeBindings[eventName] = exchangeName;
            _options.DeadLetterQueueBindings[eventName] = queueName;
            return this;
        }
    }

    /// <summary>
    /// The rabbit mq binding builder extensions class
    /// </summary>
    public static class RabbitMqBindingBuilderExtensions
    {
        /// <summary>
        /// Fors the event using the specified options
        /// </summary>
        /// <typeparam name="TEvent">The event</typeparam>
        /// <param name="options">The options</param>
        /// <returns>An event binder of t event</returns>
        public static EventBinder<TEvent> ForEvent<TEvent>(this RabbitMqBindingOption options) where TEvent : class
        {
            return new EventBinder<TEvent>(options);
        }

        /// <summary>
        /// Declares the global exchange using the specified options
        /// </summary>
        /// <param name="options">The options</param>
        /// <param name="exchangeName">The exchange name</param>
        /// <returns>The options</returns>
        public static RabbitMqBindingOption DeclareGlobalExchange(this RabbitMqBindingOption options, string exchangeName)
        {
            options.GlobalExchange = exchangeName;
            return options;
        }
        
        /// <summary>
        /// Declares the global queue using the specified options
        /// </summary>
        /// <param name="options">The options</param>
        /// <param name="queueName">The queue name</param>
        /// <returns>The options</returns>
        public static RabbitMqBindingOption DeclareGlobalQueue(this RabbitMqBindingOption options, string queueName)
        {
            options.GlobalQueue = queueName;
            return options;
        }

        /// <summary>
        /// Declares the global dead letter exchange and queue used by all events
        /// without a per-event dead letter binding.
        /// </summary>
        /// <param name="options">The options</param>
        /// <param name="exchangeName">The dead letter exchange name</param>
        /// <param name="queueName">The dead letter queue name</param>
        /// <returns>The options</returns>
        public static RabbitMqBindingOption DeclareGlobalDeadLetter(this RabbitMqBindingOption options, string exchangeName, string queueName)
        {
            options.GlobalDeadLetterExchange = exchangeName;
            options.GlobalDeadLetterQueue = queueName;
            return options;
        }
    }
}
