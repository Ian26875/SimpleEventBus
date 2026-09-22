using System.Collections;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.Generation;

namespace FluentEventBus.AsyncApi;

/// <summary>
/// Serves the document produced by <see cref="AsyncApiDocumentGenerator"/> to consumers
/// of <see cref="IAsyncApiDocumentProvider"/> — e.g. the Neuroglia AsyncAPI UI and the
/// document serving middleware. Registered automatically by AddAsyncApiDocument().
/// </summary>
public sealed class FluentEventBusDocumentProvider : IAsyncApiDocumentProvider
{
    private readonly Lazy<IAsyncApiDocument> _document;

    public FluentEventBusDocumentProvider(AsyncApiDocumentGenerator generator)
    {
        _document = new Lazy<IAsyncApiDocument>(generator.Generate);
    }

    public Task<IAsyncApiDocument?> GetDocumentAsync(string title, string version, CancellationToken cancellationToken = default)
        => Task.FromResult<IAsyncApiDocument?>(_document.Value);

    public Task<IAsyncApiDocument?> GetDocumentAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult<IAsyncApiDocument?>(_document.Value);

    public IEnumerator<IAsyncApiDocument> GetEnumerator()
    {
        yield return _document.Value;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
