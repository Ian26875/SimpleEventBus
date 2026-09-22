# EventBus Routing Design

## Topic Routing Key Schema
Format:
`{domain}.{entity}.{event}.v{version}`

Examples:
- `order.payment.created.v1`
- `user.profile.updated.v2`
- `billing.invoice.paid.v1`

### Conventions
- `domain`: bounded context (e.g. order, billing, user)
- `entity`: subject of the event (e.g. payment, profile)
- `event`: action (e.g. created, updated, deleted)
- `version`: semantic version suffix (v1, v2, ...)

### Mapping Rules
1. If `EventAttribute.Name` contains dots, it is used as the base name.
2. Otherwise, the name is derived from the event type:
   - `domain` = last segment of the namespace (fallback: `app`)
   - `entity` = PascalCase tokens excluding the last token
   - `event` = last PascalCase token

Example: `OrderPaymentCreated` in namespace `Acme.Order`
-> `order.orderpayment.created.v1`

## Exchange and Queue Naming
### Exchange
Default: `eventbus.topic`
This is a single shared topic exchange for all events.

### Queue
Default: `{service}.{environment}`
- `service` defaults to `app`
- `environment` defaults to `prod`

Multiple routing keys can bind to the same queue to form a subscription set.

## Delivery Semantics, Retry and Dead Letter

Delivery is **at-least-once**: a failed handler propagates its exception to the
transport, which retries the message. Handlers must be idempotent.

An `IEventExceptionHandler` (registered via `CatchExceptionToDo<T>()`) can set
`ExceptionContext.Handled = true` to swallow a failure — the message is then
acked and dropped without retry.

### Retry

On failure the message is redelivered with an incremented `x-retry-count`
header, up to `MaxRetryCount` (default `3`, configured on
`RabbitMqBindingOption` / `BackgroundQueueOptions`).

- **RabbitMQ**: the consumer republishes the message to its own queue via the
  default exchange with the incremented header, then acks the original delivery.
  Beyond the limit it is nacked without requeue.
- **In-Memory**: the event is re-enqueued with the same header. Beyond the limit
  `BackgroundQueueOptions.OnPoisonMessage` is invoked (if set) and the event is dropped.

### Dead Letter Exchange / Queue (RabbitMQ)

Disabled by default. When configured, the main queue is declared with
`x-dead-letter-exchange`, and the DLX (topic exchange), DLQ and their binding
(routing key `#`, since dead-lettered messages keep their original routing key)
are declared automatically.

- Global: `RabbitMqBindingOption.GlobalDeadLetterExchange` / `GlobalDeadLetterQueue`
  (DSL: `DeclareGlobalDeadLetter(exchange, queue)`)
- Per event: `ForEvent<TEvent>().WithDeadLetter(exchange, queue)` — overrides global

A message that exhausts its retries is nacked without requeue: the broker routes
it to the DLX when configured, otherwise it is dropped (an error is logged).

Note: changing the dead letter setting of an existing queue requires deleting
and re-declaring the queue — RabbitMQ rejects re-declaration with different
`x-dead-letter-exchange` arguments (`PRECONDITION_FAILED`).

## Consumer QoS / Prefetch
Default prefetch: `10`

This limits the number of unacked messages in flight per consumer to
stabilize latency under high throughput. Configure via
`RabbitMqBindingOption.PrefetchCount`.

Note: prefetch is applied on a best-effort basis using available
EasyNetQ APIs at runtime. If the underlying client does not expose
QoS on the advanced bus, the setting is ignored.
