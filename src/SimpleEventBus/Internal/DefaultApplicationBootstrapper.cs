using Microsoft.Extensions.Hosting;

namespace SimpleEventBus.Internal;

/// <summary>
/// The default application bootstrapper class
/// </summary>
/// <seealso cref="IHostedService"/>
/// <seealso cref="IApplicationBootstrapper"/>
public class DefaultApplicationBootstrapper : IHostedService, IApplicationBootstrapper
{
    /// <summary>
    /// The initializers
    /// </summary>
    private readonly IEnumerable<IInitializer> _initializers;
    
    /// <summary>
    /// The terminators
    /// </summary>
    private readonly IEnumerable<ITerminator> _terminators;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultApplicationBootstrapper"/> class
    /// </summary>
    /// <param name="initializers">The initializers</param>
    /// <param name="terminators">The terminators</param>
    public DefaultApplicationBootstrapper(IEnumerable<IInitializer> initializers, IEnumerable<ITerminator> terminators)
    {
        _initializers = initializers;
        _terminators = terminators;
    }

    /// <summary>
    /// Starts the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
    }

    /// <summary>
    /// Stops the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await TerminateAsync(cancellationToken);
    }

    /// <summary>
    /// Initializes the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The value task</returns>
    public async ValueTask InitializeAsync(CancellationToken cancellationToken)
    {
        foreach (var initializer in _initializers)
        {
            await initializer.InitializeAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Terminates the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The value task</returns>
    public async ValueTask TerminateAsync(CancellationToken cancellationToken)
    {
        foreach (var terminator in _terminators)
        {
            await terminator.TerminateAsync(cancellationToken);
        }
    }
}