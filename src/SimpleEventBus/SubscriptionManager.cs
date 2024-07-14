namespace SimpleEventBus;

/// <summary>
/// The subscription manager class
/// </summary>
/// <seealso cref="ISubscriptionManager"/>
internal class SubscriptionManager : ISubscriptionManager
{
    /// <summary>
    /// The service provider
    /// </summary>
    private readonly IServiceProvider _serviceProvider;
    /// <summary>
    /// The subscription profile
    /// </summary>
    private readonly Dictionary<string, SubscriptionProfile> _profiles = new Dictionary<string, SubscriptionProfile>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionManager"/> class
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public SubscriptionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Loads the from profile using the specified profile
    /// </summary>
    /// <typeparam name="TProfile">The profile</typeparam>
    /// <param name="profile">The profile</param>
    /// <exception cref="ArgumentException">A profile with name {profileName} is already loaded.</exception>
    public void LoadFromProfile<TProfile>(TProfile profile) where TProfile : SubscriptionProfile
    {
        var profileName = typeof(TProfile).FullName;
        if (_profiles.TryAdd(profileName, profile).Equals(false))
        {
            throw new ArgumentException($"A profile with name {profileName} is already loaded.");
        }
    }

    /// <summary>
    /// Gets the handlers for event using the specified event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    /// <exception cref="InvalidOperationException">Handler for {handlerType.Name} could not be resolved.</exception>
    /// <exception cref="InvalidOperationException">No profile found for event type {profileName}.</exception>
    /// <returns>An enumerable of i event handler t event</returns>
    public IEnumerable<IEventHandler<TEvent>> GetHandlersForEvent<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent);
        var profileName = eventType.AssemblyQualifiedName;

        if (_profiles.TryGetValue(profileName, out var profile).Equals(false))
        {
            throw new InvalidOperationException($"No profile found for event type {profileName}.");
        }

        if (profile.Handlers.TryGetValue(eventType, out var handlerTypes).Equals(false))
        {
            yield break;
        }

        foreach (var handlerType in handlerTypes)
        {
            var handler = _serviceProvider.GetService(handlerType) as IEventHandler<TEvent>;
            if (handler == null)
            {
                throw new InvalidOperationException($"Handler for {handlerType.Name} could not be resolved.");
            }
            yield return handler;
        }
    }
}
