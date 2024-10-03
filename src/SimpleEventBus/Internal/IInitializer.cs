namespace SimpleEventBus.Internal;

/// <summary>
/// The initializer interface
/// </summary>
public interface IInitializer
{
    /// <summary>
    /// Initializes the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    Task InitializeAsync(CancellationToken cancellationToken);
}