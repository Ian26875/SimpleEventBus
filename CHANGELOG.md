# Changelog

All notable changes to the FluentEventBus packages are documented here.
Format follows [Keep a Changelog](https://keepachangelog.com/); versions follow [SemVer](https://semver.org/).

## [Unreleased]

### Changed
- **BREAKING — `EventNameRegistry.Instance` removed**: the registry is created per DI
  container (no global static state), so multiple hosts in one process and parallel
  tests are fully isolated. The handler delegate cache likewise moved from static to
  invoker-instance state (the invoker is a singleton — cache lifetime and performance
  are unchanged).
- **BREAKING — `EventMapper` renamed to `EventNameRegistry`** (`IEventMapper` →
  `IEventNameRegistry`): the type is a name/type registry, not an EIP-style content
  mapper. Namespaces `FluentEventBus.Schema` and `FluentEventBus.Mapper` are unified
  into `FluentEventBus.Naming`.
- AsyncAPI document follows the canonical components layout: full message definitions
  under `components/messages` (payload/headers as top-level `$refs` into
  `components/schemas`), channel messages referencing `components/messages`.

## [0.1.0-alpha] - 2026-09-22

### Added
- **Dead letter support**: RabbitMQ DLX/DLQ via `WithDeadLetter(exchange, queue)` /
  `DeclareGlobalDeadLetter(...)`; bounded retry with `x-retry-count` header and
  `MaxRetryCount` (default 3); in-memory poison message handling via `OnPoisonMessage`.
- **Standard message headers**: `Headers.MessageId` and `Headers.OccurredAt` auto-filled
  at publish; `MessageId` is the deduplication key for at-least-once delivery.
- **Distributed tracing**: publish/consume spans (`ActivitySource "FluentEventBus"`)
  with W3C `traceparent` propagation through message headers.
- **`FluentEventBus.OpenTelemetry`** (new package, net8.0+): one-line
  `AddFluentEventBusInstrumentation()` for tracer and meter providers.
- **`FluentEventBus.AsyncApi`** (new package, net8.0+): AsyncAPI 3.0 document generation
  from subscription profiles — channels, receive operations, payload JSON Schemas with
  `required` fields, message examples (`WithExample`), servers (`WithServer`), shared
  envelope headers schema under `components/schemas`, JSON/YAML output, and a document
  provider that feeds the Neuroglia AsyncAPI UI. Sample with Docker in `samples/AsyncApiSample`.
- **Transport server discovery**: core defines `IEventBusServerDescriptor`; the InMemory
  and RabbitMQ transports register one when configured, and the AsyncAPI generator
  declares servers from them automatically (dependency-inverted — no transport
  dependency in the AsyncApi package). Explicit `WithServer()` overrides by name.
- **XML doc comments in AsyncAPI**: `WithXmlComments<T>()` turns the event type's
  `<summary>` into channel/message descriptions and property summaries into schema
  property descriptions (requires `GenerateDocumentationFile`).
- **net7.0 target** added across all packages.
- Stress test harness (`src/FluentEventBus.StressTests`): InMemory ~245k msg/s and
  RabbitMQ ~16k msg/s end-to-end measured, zero message loss.

### Changed
- **BREAKING — header wire keys unified to lowercase kebab-case**: `message-id`,
  `occurred-at`, `correlation-id`, `retry-count` (were `MessageId`, `OccurredAt`,
  `CorrelationId`, `x-retry-count`). The C# `Headers` properties are unchanged.
- **BREAKING — renamed from SimpleEventBus to FluentEventBus**: packages, repository,
  assemblies and all namespaces.
- **BREAKING — failure semantics**: unhandled handler exceptions now propagate to the
  transport (nack/retry/dead-letter) instead of being silently swallowed. An
  `IEventExceptionHandler` must set `context.Handled = true` to swallow a failure.
- **BREAKING — handler lifetime**: `ScanHandlersFrom` registers handlers as **Scoped**
  (was Singleton) and each handler executes in its own DI scope, making scoped
  dependencies (e.g. DbContext) safe. Manual registrations still win via `TryAdd`.
- **BREAKING — RabbitMQ components share one connection** via
  `RabbitMqConnectionProvider` (constructor signatures changed); exchange declarations
  are cached (one broker round-trip per exchange per process).
- Lambda handler expressions compile once at construction (was once per dispatch).
- RabbitMQ header values (`byte[]`) convert back to UTF-8 strings, fixing
  `MessageId` / `CorrelationId` round-trip.
- Test assertion library switched to AwesomeAssertions.

### Removed
- **BREAKING**: broken sync `Publish` extension (missing `this`, sync-over-async).
- **BREAKING**: `UseRabbitMq` no-op builder API (use `UseRabbitMqTransport`).
- **BREAKING**: `RabbitMqConnectionInitializer` — its fail-fast check never touched the
  broker; subscription startup topology declaration is the real connectivity check.
- Unused `Newtonsoft.Json` dependency from the RabbitMQ package.

## [0.0.1-alpha] - 2026-09-22

### Added
- Initial release as `FluentEventBus`, `FluentEventBus.InMemory`, `FluentEventBus.RabbitMq`.
- Fluent subscription profiles (`WhenOccurs<TEvent>().ToDo<THandler>()`), versioned event
  naming (`{domain}.{entity}.{event}.v{version}` via `EventAttribute`), in-memory and
  RabbitMQ (EasyNetQ) transports, multi-targeting net6.0–net10.0.
