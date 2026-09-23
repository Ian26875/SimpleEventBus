namespace FluentEventBus.Profile;

/// <summary>
///     Publish-side declaration of an event: header enrichment policies applied by the
///     publisher and documentation examples. Created via <c>Publishes&lt;TEvent&gt;()</c>.
/// </summary>
public sealed class EventPublication
{
    /// <summary>Fills the correlation-id header from the event when the caller did not set one.</summary>
    internal Func<object, string>? CorrelationIdSelector { get; set; }

    /// <summary>
    ///     Produces a deterministic message-id from the event when the caller did not set one.
    ///     The value must identify a single occurrence (consumers deduplicate on it).
    /// </summary>
    internal Func<object, string>? MessageIdSelector { get; set; }

    /// <summary>Custom header values filled from the event (fill-if-missing).</summary>
    internal List<KeyValuePair<string, Func<object, string>>> HeaderSelectors { get; } = new();

    /// <summary>Sample payloads shown as message examples in generated documentation.</summary>
    internal List<PublicationExample> Examples { get; } = new();

    internal sealed record PublicationExample(object Payload, string? Name, string? Summary);
}

/// <summary>
///     Fluent builder for a publish-side event declaration.
/// </summary>
public interface IEventPublicationBuilder<TEvent> where TEvent : class
{
    /// <summary>
    ///     Fills the <c>correlation-id</c> header from the event at publish time.
    ///     A correlation id supplied by the caller always wins.
    /// </summary>
    IEventPublicationBuilder<TEvent> WithCorrelationId(Func<TEvent, string> selector);

    /// <summary>
    ///     Produces a deterministic <c>message-id</c> from the event at publish time
    ///     (default is a random GUID). The value MUST identify a single occurrence of the
    ///     event — consumers deduplicate on it — not just the aggregate id.
    ///     A message id supplied by the caller always wins.
    /// </summary>
    IEventPublicationBuilder<TEvent> WithMessageId(Func<TEvent, string> selector);

    /// <summary>
    ///     Fills a custom header from the event at publish time (fill-if-missing).
    ///     Values are strings so they survive every transport's header typing.
    /// </summary>
    IEventPublicationBuilder<TEvent> WithHeader(string key, Func<TEvent, string> selector);

    /// <summary>
    ///     Registers a sample payload shown as a message example in generated documentation.
    /// </summary>
    IEventPublicationBuilder<TEvent> WithExample(TEvent payload, string? name = null, string? summary = null);
}

internal sealed class EventPublicationBuilder<TEvent> : IEventPublicationBuilder<TEvent> where TEvent : class
{
    private readonly EventPublication _publication;

    public EventPublicationBuilder(EventPublication publication)
    {
        _publication = publication;
    }

    public IEventPublicationBuilder<TEvent> WithCorrelationId(Func<TEvent, string> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _publication.CorrelationIdSelector = @event => selector((TEvent)@event);
        return this;
    }

    public IEventPublicationBuilder<TEvent> WithMessageId(Func<TEvent, string> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);
        _publication.MessageIdSelector = @event => selector((TEvent)@event);
        return this;
    }

    public IEventPublicationBuilder<TEvent> WithHeader(string key, Func<TEvent, string> selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(selector);
        _publication.HeaderSelectors.Add(new(key, @event => selector((TEvent)@event)));
        return this;
    }

    public IEventPublicationBuilder<TEvent> WithExample(TEvent payload, string? name = null, string? summary = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        _publication.Examples.Add(new EventPublication.PublicationExample(payload, name, summary));
        return this;
    }
}
