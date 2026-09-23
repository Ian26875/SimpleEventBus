// =============================================================================
// FluentEventBus AsyncAPI Sample
// =============================================================================
// 這個範例示範三件事：
//   1. 用 FluentEventBus 發佈/訂閱事件（In-Memory transport）
//   2. 從 SubscriptionProfile 自動產生 AsyncAPI 3.0 文件（零額外標註）
//   3. 用 Neuroglia AsyncAPI UI 呈現互動式文件頁面
//
// 端點：
//   POST /orders         發佈一筆 OrderPlaced 事件
//   GET  /asyncapi       互動式 AsyncAPI UI
//   GET  /asyncapi.json  AsyncAPI 3.0 文件（JSON）
//   GET  /asyncapi.yaml  AsyncAPI 3.0 文件（YAML）
// =============================================================================

using FluentEventBus;
using FluentEventBus.AsyncApi;
using FluentEventBus.DependencyInjection;
using FluentEventBus.Event;
using FluentEventBus.Mapper;
using FluentEventBus.Profile;
using FluentEventBus.Subscriber;
using Neuroglia.AsyncApi;

var builder = WebApplication.CreateBuilder(args);

// --- Neuroglia AsyncAPI UI ---------------------------------------------------
// UI 需要 Razor Pages；文件本身由下面的 AddAsyncApiDocument() 提供
// （FluentEventBus 已註冊 IAsyncApiDocumentProvider，UI 不需要額外接線）。
builder.Services.AddRazorPages();
builder.Services.AddAsyncApi();
builder.Services.AddAsyncApiUI();

// --- FluentEventBus ----------------------------------------------------------
builder.Services.AddEventBus(eventBus =>
{
    // 訂閱設定：哪個事件由哪個 handler 處理（見檔尾 OrderProfile）
    eventBus.WithProfile<OrderProfile>();
    // 掃描本組件內的 IEventHandler<T> 實作並註冊進 DI（Scoped）
    eventBus.ScanHandlersFrom(typeof(OrderProfile).Assembly);
    // delegate 綁定的 service 不會被掃描，要自己註冊
    eventBus.Services.AddScoped<IOrderNotificationService, OrderNotificationService>();
    // In-Process transport；正式環境換成 UseRabbitMqTransport(...)
    eventBus.UseInMemoryTransport();

    // AsyncAPI 文件：channels/operations/schemas 全部從上面的 profile 推導
    eventBus.AddAsyncApiDocument(options =>
    {
        // info 區塊（文件標題/版本/描述）
        options.Title = "Orders Service";
        options.Version = "1.0.0";
        options.Description = "Sample service demonstrating FluentEventBus with AsyncAPI generation.";

        // 讀取 XML 文件註解（需 csproj 開 GenerateDocumentationFile）：
        // 事件型別的 <summary> → channel/message 描述；屬性 <summary> → schema 屬性描述
        options.WithXmlComments<OrderPlaced>();

        // servers 區塊會自動從已設定的 transport 探索（IEventBusServerDescriptor）；
        // 也可以用 options.WithServer("production", "rabbitmq.internal:5672", "amqp") 明確宣告/覆蓋

        // 訊息範例：顯示在文件的 message examples
        options.WithExample(
            new OrderPlaced { OrderId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"), PlacedAt = DateTimeOffset.Parse("2026-09-22T10:00:00+08:00") },
            name: "typical-order",
            summary: "A typical order placed during business hours.");
    });
});

var app = builder.Build();

// 發佈事件：curl -X POST http://localhost:8081/orders
// MessageId / OccurredAt 會由 publisher 自動填入 headers
app.MapPost("/orders", async (IEventPublisher publisher) =>
{
    var @event = new OrderPlaced { OrderId = Guid.NewGuid(), PlacedAt = DateTimeOffset.UtcNow };
    await publisher.PublishAsync(@event);
    return Results.Accepted(value: @event);
});

// AsyncAPI 3.0 文件（JSON）：curl http://localhost:8081/asyncapi.json
app.MapGet("/asyncapi.json", async (AsyncApiDocumentGenerator generator, CancellationToken cancellationToken) =>
{
    var json = await generator.SerializeAsync(AsyncApiDocumentFormat.Json, cancellationToken);
    return Results.Content(json, "application/json");
});

// 同一份文件的 YAML 版：curl http://localhost:8081/asyncapi.yaml
app.MapGet("/asyncapi.yaml", async (AsyncApiDocumentGenerator generator, CancellationToken cancellationToken) =>
{
    var yaml = await generator.SerializeAsync(AsyncApiDocumentFormat.Yaml, cancellationToken);
    return Results.Content(yaml, "text/yaml");
});

// 互動式 AsyncAPI UI（Razor Pages）：http://localhost:8081/asyncapi
app.MapRazorPages();

app.Run();

// --- 事件（wire contract）----------------------------------------------------
// [Event] 決定 routing key（channel address）：shop.order.placed.v1
// 型別與屬性的 <summary> 會透過 WithXmlComments 進入 AsyncAPI 文件

/// <summary>
/// Raised when a customer places an order and payment has been authorized.
/// </summary>
[Event("shop.order.placed", 1)]
public sealed record OrderPlaced
{
    /// <summary>Unique identifier of the placed order.</summary>
    public required Guid OrderId { get; init; }

    /// <summary>When the order was placed (UTC).</summary>
    public required DateTimeOffset PlacedAt { get; init; }
}

// --- Handler（consumer 端）---------------------------------------------------
// 以 Scoped 生命週期執行，每次 dispatch 有自己的 DI scope（可安全注入 DbContext）。
// 失敗會由 transport 重試（預設 3 次），超過即進 dead letter / poison 流程。
public sealed class OrderPlacedHandler : IEventHandler<OrderPlaced>
{
    private readonly ILogger<OrderPlacedHandler> _logger;

    public OrderPlacedHandler(ILogger<OrderPlacedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderPlaced @event, Headers headers, CancellationToken cancellationToken)
    {
        // headers.MessageId 是 at-least-once 去重的依據
        _logger.LogInformation("Order {OrderId} placed at {PlacedAt} (MessageId: {MessageId})",
            @event.OrderId, @event.PlacedAt, headers.MessageId);
        return Task.CompletedTask;
    }
}

// --- Delegate 綁定（Func<> 形式）----------------------------------------------
// 不實作 IEventHandler<T> 也能訂閱：任何已註冊 service 上簽名相符的方法
// （(TEvent, Headers, CancellationToken) => Task 或 (TEvent, CancellationToken) => Task）
// 都可以用 ToDo<TService>(s => s.Method) 綁定。
public interface IOrderNotificationService
{
    Task PushAsync(OrderPlaced @event, Headers headers, CancellationToken cancellationToken);
}

public sealed class OrderNotificationService : IOrderNotificationService
{
    private readonly ILogger<OrderNotificationService> _logger;

    public OrderNotificationService(ILogger<OrderNotificationService> logger)
    {
        _logger = logger;
    }

    public Task PushAsync(OrderPlaced @event, Headers headers, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Func<> binding] Push notification for order {OrderId} (MessageId: {MessageId})",
            @event.OrderId, headers.MessageId);
        return Task.CompletedTask;
    }
}

// --- Subscription Profile ----------------------------------------------------
// 事件 → handler 的對照表；AsyncAPI 的 receive operations 也是從這裡推導
public sealed class OrderProfile : SubscriptionProfile
{
    public OrderProfile()
    {
        this.WhenOccurs<OrderPlaced>()
            .ToDo<OrderPlacedHandler>()                             // 介面實作
            .ToDo<IOrderNotificationService>(s => s.PushAsync);     // Func<> delegate 綁定
    }
}
