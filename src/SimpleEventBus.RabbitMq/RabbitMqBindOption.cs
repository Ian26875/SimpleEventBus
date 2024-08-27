namespace SimpleEventBus.RabbitMq
{
    /// <summary>
    /// The rabbit mq binding option class
    /// </summary>
    public class RabbitMqBindingOption
    {
        /// <summary>
        /// Gets or sets the value of the exchange bindings
        /// </summary>
        public Dictionary<Type, string> ExchangeBindings { get; set; } = new Dictionary<Type, string>();

        /// <summary>
        /// Gets or sets the value of the queue bindings
        /// </summary>
        public Dictionary<Type, string> QueueBindings { get; set; } = new Dictionary<Type, string>();
        
        /// <summary>
        /// Gets or sets the global exchange name
        /// </summary>
        public string GlobalExchange { get; set; }

        /// <summary>
        /// Gets or sets the global queue name
        /// </summary>
        public string GlobalQueue { get; set; }
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
            _options.ExchangeBindings[typeof(TEvent)] = exchangeName;
            return this;
        }

        /// <summary>
        /// Declares the queue using the specified queue name
        /// </summary>
        /// <param name="queueName">The queue name</param>
        /// <returns>An event binder of t event</returns>
        public EventBinder<TEvent> DeclareQueue(string queueName)
        {
            _options.QueueBindings[typeof(TEvent)] = queueName;
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
    }
}