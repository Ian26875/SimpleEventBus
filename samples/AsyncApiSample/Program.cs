using FluentEventBus;
using FluentEventBus.AsyncApi;
using FluentEventBus.DependencyInjection;
using FluentEventBus.Event;
using FluentEventBus.Mapper;
using FluentEventBus.Profile;
using FluentEventBus.Subscriber;
using Neuroglia.AsyncApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEventBus(eventBus =>
{
    eventBus.WithProfile<OrderProfile>();
    eventBus.ScanHandlersFrom(typeof(OrderProfile).Assembly);
    eventBus.UseInMemoryTransport();

    // AsyncAPI document generated from the profiles above — no extra annotations.
    eventBus.AddAsyncApiDocument(options =>
    {
        options.Title = "Orders Service";
        options.Version = "1.0.0";
        options.Description = "Sample service demonstrating FluentEventBus with AsyncAPI generation.";
        options.WithExample(
            new OrderPlaced(Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"), DateTimeOffset.Parse("2026-09-22T10:00:00+08:00")),
            name: "typical-order",
            summary: "A typical order placed during business hours.");
    });
});

var app = builder.Build();

// Publish an event: curl -X POST http://localhost:5000/orders
app.MapPost("/orders", async (IEventPublisher publisher) =>
{
    var @event = new OrderPlaced(Guid.NewGuid(), DateTimeOffset.UtcNow);
    await publisher.PublishAsync(@event);
    return Results.Accepted(value: @event);
});

// The AsyncAPI 3.0 contract: curl http://localhost:5000/asyncapi.json
app.MapGet("/asyncapi.json", async (AsyncApiDocumentGenerator generator, CancellationToken cancellationToken) =>
{
    var json = await generator.SerializeAsync(AsyncApiDocumentFormat.Json, cancellationToken);
    return Results.Content(json, "application/json");
});

// Same document as YAML: curl http://localhost:5000/asyncapi.yaml
app.MapGet("/asyncapi.yaml", async (AsyncApiDocumentGenerator generator, CancellationToken cancellationToken) =>
{
    var yaml = await generator.SerializeAsync(AsyncApiDocumentFormat.Yaml, cancellationToken);
    return Results.Content(yaml, "text/yaml");
});

app.Run();

[Event("order.order.placed", 1)]
public sealed record OrderPlaced(Guid OrderId, DateTimeOffset PlacedAt);

public sealed class OrderPlacedHandler : IEventHandler<OrderPlaced>
{
    private readonly ILogger<OrderPlacedHandler> _logger;

    public OrderPlacedHandler(ILogger<OrderPlacedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderPlaced @event, Headers headers, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order {OrderId} placed at {PlacedAt} (MessageId: {MessageId})",
            @event.OrderId, @event.PlacedAt, headers.MessageId);
        return Task.CompletedTask;
    }
}

public sealed class OrderProfile : SubscriptionProfile
{
    public OrderProfile()
    {
        this.WhenOccurs<OrderPlaced>().ToDo<OrderPlacedHandler>();
    }
}
