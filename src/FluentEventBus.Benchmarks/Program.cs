using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FluentEventBus;
using FluentEventBus.DependencyInjection;
using FluentEventBus.Event;
using FluentEventBus.Naming;
using FluentEventBus.Profile;
using FluentEventBus.Serialization;
using FluentEventBus.Subscriber;
using Microsoft.Extensions.DependencyInjection;

BenchmarkRunner.Run<EventBusBenchmarks>(args: args);

/// <summary>
/// Micro-benchmarks of the FluentEventBus hot paths: publish pipeline
/// (serialize + envelope headers + tracing hooks, transport stubbed out) and
/// handler dispatch (cached compiled delegates + per-handler DI scope).
/// Complements src/FluentEventBus.StressTests, which measures end-to-end throughput.
/// </summary>
[MemoryDiagnoser]
[ShortRunJob]
public class EventBusBenchmarks
{
    private IEventHandlerInvoker _invoker = null!;
    private NullTransportPublisher _publisher = null!;
    private EventNameRegistry _registry = null!;
    private BenchSingle _singleEvent = null!;
    private BenchDelegate _delegateEvent = null!;
    private BenchMulti _multiEvent = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEventBus(builder =>
        {
            builder.WithProfile<BenchProfile>();
            builder.ScanHandlersFrom(typeof(BenchProfile).Assembly);
        });
        services.AddSingleton<IBenchNotifier, BenchNotifier>();

        var provider = services.BuildServiceProvider();
        ((SubscriptionProfileManager)provider.GetRequiredService<ISubscriptionProfileManager>()).Initialize();

        _invoker = provider.GetRequiredService<IEventHandlerInvoker>();
        _publisher = new NullTransportPublisher(
            provider.GetRequiredService<ISerializer>(),
            provider.GetRequiredService<IEventNameRegistry>());

        _registry = new EventNameRegistry();
        _registry.Register(typeof(BenchSingle));

        _singleEvent = new BenchSingle(Guid.NewGuid(), "payload");
        _delegateEvent = new BenchDelegate(Guid.NewGuid());
        _multiEvent = new BenchMulti(Guid.NewGuid());
    }

    /// <summary>Full publish pipeline with the transport stubbed to a no-op.</summary>
    [Benchmark]
    public Task Publish_CorePipeline() => _publisher.PublishAsync(_singleEvent);

    /// <summary>One interface handler: cached delegate + one DI scope.</summary>
    [Benchmark]
    public Task Dispatch_SingleInterfaceHandler() => _invoker.InvokeAsync(_singleEvent, new Headers());

    /// <summary>One Func&lt;&gt; (expression) bound handler.</summary>
    [Benchmark]
    public Task Dispatch_DelegateHandler() => _invoker.InvokeAsync(_delegateEvent, new Headers());

    /// <summary>Three handlers on one event (Parallel.ForEachAsync path, per-handler scopes).</summary>
    [Benchmark]
    public Task Dispatch_ThreeHandlers() => _invoker.InvokeAsync(_multiEvent, new Headers());

    /// <summary>Type → versioned name lookup (registry hit).</summary>
    [Benchmark]
    public string Registry_GetEventName() => _registry.GetEventName(typeof(BenchSingle));
}

public sealed record BenchSingle(Guid Id, string Name);
public sealed record BenchDelegate(Guid Id);
public sealed record BenchMulti(Guid Id);

public sealed class BenchSingleHandler : IEventHandler<BenchSingle>
{
    public Task HandleAsync(BenchSingle @event, Headers headers, CancellationToken cancellationToken) => Task.CompletedTask;
}

public interface IBenchNotifier
{
    Task NotifyAsync(BenchDelegate @event, Headers headers, CancellationToken cancellationToken);
}

public sealed class BenchNotifier : IBenchNotifier
{
    public Task NotifyAsync(BenchDelegate @event, Headers headers, CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class BenchMultiHandlerA : IEventHandler<BenchMulti>
{
    public Task HandleAsync(BenchMulti @event, Headers headers, CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class BenchMultiHandlerB : IEventHandler<BenchMulti>
{
    public Task HandleAsync(BenchMulti @event, Headers headers, CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class BenchMultiHandlerC : IEventHandler<BenchMulti>
{
    public Task HandleAsync(BenchMulti @event, Headers headers, CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class BenchProfile : SubscriptionProfile
{
    public BenchProfile()
    {
        this.WhenOccurs<BenchSingle>().ToDo<BenchSingleHandler>();
        this.WhenOccurs<BenchDelegate>().ToDo<IBenchNotifier>(n => n.NotifyAsync);
        this.WhenOccurs<BenchMulti>()
            .ToDo<BenchMultiHandlerA>()
            .ToDo<BenchMultiHandlerB>()
            .ToDo<BenchMultiHandlerC>();
    }
}

/// <summary>Publisher with the transport stubbed out, isolating the core publish pipeline.</summary>
public sealed class NullTransportPublisher : AbstractEventPublisher
{
    public NullTransportPublisher(ISerializer serializer, IEventNameRegistry registry) : base(serializer, registry)
    {
    }

    protected override Task PublishEventAsync(EventContext eventContext, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
