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

## Consumer QoS / Prefetch
Default prefetch: `10`

This limits the number of unacked messages in flight per consumer to
stabilize latency under high throughput. Configure via
`RabbitMqBindingOption.PrefetchCount`.

Note: prefetch is applied on a best-effort basis using available
EasyNetQ APIs at runtime. If the underlying client does not expose
QoS on the advanced bus, the setting is ignored.
