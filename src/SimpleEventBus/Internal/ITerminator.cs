namespace SimpleEventBus.Internal;

/// <summary>
/// The terminator interface
/// </summary>
public interface ITerminator
{
    /// <summary>
    /// Terminates the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    Task TerminateAsync(CancellationToken cancellationToken);
}