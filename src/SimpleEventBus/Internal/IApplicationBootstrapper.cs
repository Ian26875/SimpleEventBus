namespace SimpleEventBus.Internal;

/// <summary>
/// The application bootstrapper interface
/// </summary>
public interface IApplicationBootstrapper
{
    /// <summary>
    /// Initializes and starts the application components.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The value task</returns>
    ValueTask InitializeAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Shuts down and cleans up the application components.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The value task</returns>
    ValueTask TerminateAsync(CancellationToken cancellationToken);
}