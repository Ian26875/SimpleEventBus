using FluentEventBus.Event;
using FluentEventBus.Profile;
using FluentEventBus.Naming;
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
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;
    private readonly IEventNameRegistry _eventMapper;
    private readonly AsyncApiDocumentOptions _options;
    private readonly IEnumerable<IEventBusServerDescriptor> _serverDescriptors;
    private readonly IAsyncApiDocumentWriter _documentWriter;
    private readonly SchemaGeneratorConfiguration _schemaConfiguration;
    private readonly HashSet<string> _xmlCommentFiles = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _typeSummaries = new();
    private bool _xmlCommentsPrepared;

    public AsyncApiDocumentGenerator(ISubscriptionProfileManager subscriptionProfileManager,
                                     IEventNameRegistry eventMapper,
                                     IOptions<AsyncApiDocumentOptions> options,
                                     IEnumerable<IEventBusServerDescriptor> serverDescriptors,
                                     IAsyncApiDocumentWriter documentWriter)
    {
        _subscriptionProfileManager = subscriptionProfileManager;
        _eventMapper = eventMapper;
        _options = options.Value;
        _serverDescriptors = serverDescriptors;
        _documentWriter = documentWriter;

        // StringFormatSchemaGenerator: System.Text.Json serializes these types as strings,
        // but the schema generator would otherwise reflect them structurally (Day/Hour/Ticks...).
        _schemaConfiguration = new SchemaGeneratorConfiguration
        {
            Generators = { new StringFormatSchemaGenerator() },
            Refiners = { new RequiredPropertiesRefiner() }
        };
        foreach (var configure in _options.SchemaConfigurators)
        {
            configure(_schemaConfiguration);
        }

        foreach (var file in _options.XmlCommentFiles)
        {
            _xmlCommentFiles.Add(file);
        }
    }

    /// <summary>
    /// Discovers XML documentation files automatically: the entry assembly and every
    /// registered event type's assembly are probed for an XML file next to the DLL,
    /// so descriptions flow into the document without any WithXmlComments() call.
    /// Explicitly registered files (WithXmlComments) are kept and never re-added.
    /// </summary>
    private void PrepareXmlComments()
    {
        if (_xmlCommentsPrepared)
        {
            return;
        }

        _xmlCommentsPrepared = true;

        var assemblies = _subscriptionProfileManager.GetAllSubscriptions().Keys
            .Select(eventType => eventType.Assembly)
            .Distinct()
            .ToList();
        var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
        if (entryAssembly is not null && !assemblies.Contains(entryAssembly))
        {
            assemblies.Add(entryAssembly);
        }

        var registerMethod = typeof(SchemaGeneratorConfiguration).GetMethod(nameof(SchemaGeneratorConfiguration.RegisterXmlCommentFile));
        foreach (var assembly in assemblies)
        {
            if (string.IsNullOrEmpty(assembly.Location))
            {
                continue;
            }

            var xmlPath = Path.ChangeExtension(assembly.Location, ".xml");
            if (!File.Exists(xmlPath) || !_xmlCommentFiles.Add(xmlPath))
            {
                continue;
            }

            var markerType = assembly.GetExportedTypes().FirstOrDefault() ?? assembly.GetTypes().FirstOrDefault();
            if (markerType is not null && registerMethod is not null)
            {
                registerMethod.MakeGenericMethod(markerType).Invoke(_schemaConfiguration, new object[] { xmlPath });
            }
        }

        _typeSummaries = LoadTypeSummaries(_xmlCommentFiles);
    }

    /// <summary>
    /// Loads &lt;summary&gt; texts from the registered XML documentation files,
    /// keyed by doc id ("T:Full.Type.Name" for types, "P:Full.Type.Name.Property" for properties).
    /// </summary>
    private static Dictionary<string, string> LoadTypeSummaries(IEnumerable<string> xmlFiles)
    {
        var summaries = new Dictionary<string, string>();
        foreach (var file in xmlFiles.Where(File.Exists))
        {
            var document = System.Xml.Linq.XDocument.Load(file);
            foreach (var member in document.Descendants("member"))
            {
                var name = member.Attribute("name")?.Value;
                var summary = member.Element("summary")?.Value.Trim();
                if (name is not null && (name.StartsWith("T:") || name.StartsWith("P:")) && !string.IsNullOrWhiteSpace(summary))
                {
                    summaries[name] = summary;
                }
            }
        }

        return summaries;
    }

    private string? GetTypeSummary(Type type)
    {
        var docId = "T:" + type.FullName?.Replace('+', '.');
        return _typeSummaries.TryGetValue(docId, out var summary) ? summary : null;
    }

    /// <summary>
    /// Builds the payload schema and injects property descriptions from the XML
    /// documentation comments (property &lt;summary&gt;). The result is materialized
    /// back into a <see cref="JsonSchema"/> so consumers that render schemas
    /// (e.g. the Neuroglia AsyncAPI UI) can process it.
    /// </summary>
    private JsonSchema BuildPayloadSchema(Type eventType)
    {
        var schema = new JsonSchemaBuilder().FromType(eventType, _schemaConfiguration).Build();
        var node = System.Text.Json.JsonSerializer.SerializeToNode(schema)!;

        var injected = false;
        if (node["properties"] is System.Text.Json.Nodes.JsonObject properties)
        {
            var typeDocId = eventType.FullName?.Replace('+', '.');
            foreach (var (propertyName, propertySchema) in properties)
            {
                if (propertySchema is System.Text.Json.Nodes.JsonObject propertyObject
                    && propertyObject["description"] is null
                    && _typeSummaries.TryGetValue($"P:{typeDocId}.{propertyName}", out var summary))
                {
                    propertyObject["description"] = summary;
                    injected = true;
                }
            }
        }

        return injected ? JsonSchema.FromText(node.ToJsonString()) : schema;
    }

    /// <summary>
    /// Builds the AsyncAPI document from the registered subscription profiles.
    /// Safe to call without starting the host or connecting to a broker.
    /// </summary>
    public V3AsyncApiDocument Generate()
    {
        _subscriptionProfileManager.Initialize();
        PrepareXmlComments();

        var entryAssemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName();

        var document = new V3AsyncApiDocument
        {
            AsyncApi = "3.0.0",
            Info = new V3ApiInfo
            {
                Title = _options.Title ?? entryAssemblyName?.Name ?? "FluentEventBus Application",
                Version = _options.Version ?? entryAssemblyName?.Version?.ToString(3) ?? "1.0.0",
                Description = _options.Description
            },
            DefaultContentType = "application/json",
            Channels = [],
            Operations = []
        };

        // Canonical components layout: payload schemas under components/schemas,
        // full message definitions under components/messages, channels referencing
        // components/messages — the structure recommended by the AsyncAPI docs.
        document.Components = new()
        {
            Schemas = new()
            {
                [EventBusHeadersSchemaName] = new V3SchemaDefinition { Schema = BuildHeadersSchema() }
            },
            Messages = new()
        };

        // Servers: transport-registered descriptors first (dependency-inverted via
        // IEventBusServerDescriptor), explicit WithServer() entries override by name.
        var servers = _serverDescriptors.ToDictionary(
            descriptor => descriptor.Name,
            descriptor => new AsyncApiDocumentOptions.ServerInfo(
                descriptor.Host, descriptor.Protocol, descriptor.Description, descriptor.ProtocolVersion));
        foreach (var (name, server) in _options.Servers)
        {
            servers[name] = server;
        }

        var serverReferences = new Neuroglia.EquatableList<V3ReferenceDefinition>();
        if (servers.Count > 0)
        {
            document.Servers = [];
            foreach (var (name, server) in servers)
            {
                document.Servers[name] = new V3ServerDefinition
                {
                    Host = server.Host,
                    Protocol = server.Protocol,
                    ProtocolVersion = server.ProtocolVersion,
                    Description = server.Description
                };
                serverReferences.Add(new V3ReferenceDefinition { Reference = $"#/servers/{name}" });
            }
        }

        foreach (var (eventType, executors) in _subscriptionProfileManager.GetAllSubscriptions())
        {
            var eventName = _eventMapper.GetEventName(eventType);
            var messageName = eventType.Name;

            // Event body schema: one named definition under components/schemas.
            document.Components.Schemas![messageName] = new V3SchemaDefinition
            {
                Schema = BuildPayloadSchema(eventType)
            };

            var typeSummary = GetTypeSummary(eventType);

            // Full message definition under components/messages; payload and headers
            // reference their component schemas (top-level $ref — dereferenced by
            // renderers such as the Neuroglia UI).
            var message = new V3MessageDefinition
            {
                Name = messageName,
                Title = messageName,
                Description = string.IsNullOrWhiteSpace(typeSummary) ? null : typeSummary,
                ContentType = "application/json",
                Payload = new V3SchemaDefinition { Reference = $"#/components/schemas/{messageName}" },
                Headers = new V3SchemaDefinition { Reference = $"#/components/schemas/{EventBusHeadersSchemaName}" }
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

            document.Components.Messages![messageName] = message;

            document.Channels[eventName] = new V3ChannelDefinition
            {
                Address = eventName,
                Description = string.IsNullOrWhiteSpace(typeSummary) ? null : typeSummary,
                // The channel message is a $ref into components/messages.
                Messages = new()
                {
                    [messageName] = new V3MessageDefinition { Reference = $"#/components/messages/{messageName}" }
                },
                // Reference every declared server explicitly (the Neuroglia UI expects a non-null list).
                Servers = serverReferences
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

    /// <summary>Name of the shared headers schema under components/schemas.</summary>
    internal const string EventBusHeadersSchemaName = "eventBusHeaders";

    /// <summary>
    /// Schema of the standard FluentEventBus envelope headers
    /// (see Headers: MessageId / OccurredAt auto-filled at publish, CorrelationId caller-set,
    /// retry-count added by the transport during redelivery).
    /// </summary>
    private static JsonSchema BuildHeadersSchema()
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Properties(
                (Headers.MessageIdKey, new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Format(Formats.Uuid)
                    .Description("Unique message id, auto-assigned at publish. Deduplication key for at-least-once delivery.")),
                (Headers.OccurredAtKey, new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Format(Formats.DateTime)
                    .Description("UTC time the event was published (ISO-8601), auto-assigned at publish.")),
                (Headers.CorrelationIdKey, new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Description("Caller-set id for end-to-end tracing across services.")),
                (Headers.RetryCountKey, new JsonSchemaBuilder()
                    .Type(SchemaValueType.Integer)
                    .Description("Redelivery counter added by the transport during the retry/dead-letter flow.")))
            .Required(Headers.MessageIdKey, Headers.OccurredAtKey)
            .Build();
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

    /// <summary>
    /// Marks non-nullable properties (value types and non-nullable reference types)
    /// as required, matching what the serializer actually guarantees on the wire.
    /// </summary>
    private sealed class RequiredPropertiesRefiner : ISchemaRefiner
    {
        public bool ShouldRun(SchemaGenerationContextBase context)
        {
            return context.Type is { IsClass: true } or { IsValueType: true, IsPrimitive: false }
                   && context.Type != typeof(string)
                   && context.Intents.Any(intent => intent is PropertiesIntent);
        }

        public void Run(SchemaGenerationContextBase context)
        {
            var nullability = new System.Reflection.NullabilityInfoContext();
            var required = context.Type
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                .Where(property => nullability.Create(property).ReadState == System.Reflection.NullabilityState.NotNull)
                .Select(property => property.Name)
                .ToList();

            if (required.Count > 0)
            {
                context.Intents.Add(new RequiredIntent(required));
            }
        }
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
