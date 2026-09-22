namespace FluentEventBus;

/// <summary>
/// Transport-neutral description of a server (broker) the event bus talks to.
/// Transports register one when configured; documentation tooling (e.g. the
/// AsyncAPI generator) consumes them without depending on any transport package.
/// </summary>
public interface IEventBusServerDescriptor
{
    /// <summary>Server key, e.g. "rabbitmq".</summary>
    string Name { get; }

    /// <summary>Host and optional port, without protocol prefix, e.g. "rabbitmq.internal:5672".</summary>
    string Host { get; }

    /// <summary>Protocol name, e.g. "amqp", "inmemory".</summary>
    string Protocol { get; }

    /// <summary>Optional protocol version, e.g. "0.9.1".</summary>
    string? ProtocolVersion { get; }

    /// <summary>Optional human-readable description.</summary>
    string? Description { get; }
}

/// <summary>
/// Default immutable implementation of <see cref="IEventBusServerDescriptor"/>.
/// </summary>
public sealed record EventBusServerDescriptor(
    string Name,
    string Host,
    string Protocol,
    string? ProtocolVersion = null,
    string? Description = null) : IEventBusServerDescriptor;
