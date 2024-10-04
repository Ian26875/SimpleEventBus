using Microsoft.Extensions.Logging;
using SimpleEventBus.Profile;
using SimpleEventBus.Schema;

namespace SimpleEventBus.Internal;

public class CheckSubscriptionProfileManager : IInitializer
{
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    private readonly ILogger<CheckSubscriptionProfileManager> _logger;

    private readonly ISchemaRegistry _schemaRegistry;
    
    public CheckSubscriptionProfileManager(ISubscriptionProfileManager subscriptionProfileManager, ILogger<CheckSubscriptionProfileManager> logger, ISchemaRegistry schemaRegistry)
    {
        _subscriptionProfileManager = subscriptionProfileManager;
        _logger = logger;
        _schemaRegistry = schemaRegistry;
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        var eventEventHandlers = _subscriptionProfileManager.GetAllEventHandlers();
       
        this._logger.LogInformation($"register {eventEventHandlers.Keys.Count} events.");
        
        return Task.CompletedTask;
    }
}