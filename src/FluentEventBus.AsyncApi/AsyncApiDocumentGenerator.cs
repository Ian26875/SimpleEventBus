using FluentEventBus.Profile;
using FluentEventBus.Schema;
using Json.Schema;
using Json.Schema.Generation;
using Json.Schema.Generation.Generators;
using Json.Schema.Generation.Intents;
using Microsoft.Extensions.Options;
using Neuroglia.AsyncApi;
using Neuroglia.AsyncApi.IO;
using Neuroglia.AsyncApi.v3;

namespace FluentEventBus.AsyncApi;

/// <summary>
/// Builds an AsyncAPI 3.0 document from the FluentEventBus subscription profiles:
/// every registered event becomes a channel (address = versioned event name) with its
/// payload JSON Schema, and every handler subscription becomes a receive operation.
/// No extra annotations are required — the profiles are the single source of truth.
/// </summary>
public sealed class AsyncApiDocumentGenerator
{
    /// <summary>
    /// System.Text.Json serializes these types as strings, but the schema generator
    /// would otherwise reflect them structurally (Day/Hour/Ticks...).
    /// </summary>
    private static readonly SchemaGeneratorConfiguration SchemaConfiguration = new()
    {
        Generators = { new StringFormatSchemaGenerator() }
    };

    private readonly ISubscriptionProfileManager _subscriptionProfileManager;
    private readonly IEventMapper _eventMapper;
    private readonly AsyncApiDocumentOptions _options;
    private readonly IAsyncApiDocumentWriter _documentWriter;

    public AsyncApiDocumentGenerator(ISubscriptionProfileManager subscriptionProfileManager,
                                     IEventMapper eventMapper,
                                     IOptions<AsyncApiDocumentOptions> options,
                                     IAsyncApiDocumentWriter documentWriter)
    {
        _subscriptionProfileManager = subscriptionProfileManager;
        _eventMapper = eventMapper;
        _options = options.Value;
        _documentWriter = documentWriter;
    }

    /// <summary>
    /// Builds the AsyncAPI document from the registered subscription profiles.
    /// Safe to call without starting the host or connecting to a broker.
    /// </summary>
    public V3AsyncApiDocument Generate()
    {
        _subscriptionProfileManager.Initialize();

        var document = new V3AsyncApiDocument
        {
            AsyncApi = "3.0.0",
            Info = new V3ApiInfo
            {
                Title = _options.Title,
                Version = _options.Version,
                Description = _options.Description
            },
            DefaultContentType = "application/json",
            Channels = [],
            Operations = []
        };

        foreach (var (eventType, executors) in _subscriptionProfileManager.GetAllSubscriptions())
        {
            var eventName = _eventMapper.GetEventName(eventType);
            var messageName = eventType.Name;

            var payloadSchema = new JsonSchemaBuilder().FromType(eventType, SchemaConfiguration).Build();

            var message = new V3MessageDefinition
            {
                Name = messageName,
                Title = messageName,
                ContentType = "application/json",
                Payload = new V3SchemaDefinition { Schema = payloadSchema }
            };

            if (_options.Examples.TryGetValue(eventType, out var examples))
            {
                message.Examples = new(examples.Select(example => new V3MessageExampleDefinition
                {
                    Name = example.Name,
                    Summary = example.Summary,
                    Payload = ToExamplePayload(example.Payload)
                }));
            }

            document.Channels[eventName] = new V3ChannelDefinition
            {
                Address = eventName,
                Messages = new() { [messageName] = message }
            };

            var handlerNames = executors.Select(executor => executor.HandlerType.Name).Distinct();
            document.Operations[$"receive.{eventName}"] = new V3OperationDefinition
            {
                Action = V3OperationAction.Receive,
                Channel = new V3ReferenceDefinition { Reference = $"#/channels/{eventName}" },
                Summary = $"Handled by: {string.Join(", ", handlerNames)}",
                Messages =
                [
                    new V3ReferenceDefinition { Reference = $"#/channels/{eventName}/messages/{messageName}" }
                ]
            };
        }

        return document;
    }

    /// <summary>
    /// Generates the document and serializes it to JSON or YAML.
    /// </summary>
    public async Task<string> SerializeAsync(AsyncApiDocumentFormat format = AsyncApiDocumentFormat.Json,
                                             CancellationToken cancellationToken = default)
    {
        var document = Generate();
        using var stream = new MemoryStream();
        await _documentWriter.WriteAsync(document, stream, format, cancellationToken);
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Converts a sample event instance into the property dictionary the
    /// example definition expects, using the same JSON shape as the wire format.
    /// </summary>
    private static Neuroglia.EquatableDictionary<string, object> ToExamplePayload(object payload)
    {
        var jsonObject = System.Text.Json.JsonSerializer.SerializeToNode(payload)!.AsObject();
        var dictionary = new Neuroglia.EquatableDictionary<string, object>();
        foreach (var property in jsonObject)
        {
            dictionary[property.Key] = property.Value!;
        }

        return dictionary;
    }

    private sealed class StringFormatSchemaGenerator : ISchemaGenerator
    {
        public bool Handles(Type type)
        {
            return type == typeof(DateTimeOffset) || type == typeof(TimeSpan);
        }

        public void AddConstraints(SchemaGenerationContextBase context)
        {
            context.Intents.Add(new TypeIntent(SchemaValueType.String));
            context.Intents.Add(new FormatIntent(
                context.Type == typeof(TimeSpan) ? Formats.Duration : Formats.DateTime));
        }
    }
}
