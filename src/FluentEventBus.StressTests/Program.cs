using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentEventBus;
using FluentEventBus.DependencyInjection;
using FluentEventBus.Event;
using FluentEventBus.Profile;
using FluentEventBus.Subscriber;

// Usage:
//   dotnet run -c Release -- inmemory [count]
//   dotnet run -c Release -- rabbitmq <host[:port]> [count]
var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "inmemory";
var rabbitHost = mode == "rabbitmq"
    ? (args.Length > 1 ? args[1] : "localhost")
    : string.Empty;
var count = int.TryParse(args.LastOrDefault(), out var parsed)
    ? parsed
    : mode == "rabbitmq" ? 10_000 : 100_000;

Console.WriteLine($"[stress] mode={mode} count={count:N0}");

var builder = Host.CreateApplicationBuilder();
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Services.AddEventBus(eventBus =>
{
    eventBus.WithProfile<StressProfile>();
    eventBus.ScanHandlersFrom(typeof(StressProfile).Assembly);

    if (mode == "rabbitmq")
    {
        eventBus.UseRabbitMqTransport(
            option =>
            {
                option.Host = rabbitHost;
                option.UserName = "guest";
                option.Password = "guest";
            },
            bind =>
            {
                bind.GlobalExchange = "stress.topic";
                bind.GlobalQueue = "stress.queue";
                bind.PrefetchCount = 200;
            });
    }
    else
    {
        eventBus.UseInMemoryTransport(capacity: count + 1000, alertThreshold: count + 999);
    }
});

var host = builder.Build();
await host.StartAsync();

// Give the RabbitMQ consumer a moment to finish declaring topology.
if (mode == "rabbitmq")
{
    await Task.Delay(1000);
}

StressCounter.Reset(count);
var publisher = host.Services.GetRequiredService<IEventPublisher>();

var total = Stopwatch.StartNew();
await Parallel.ForAsync(0, count, new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (i, _) =>
{
    await publisher.PublishAsync(new StressEvent(i));
});
var publishElapsed = total.Elapsed;

var completed = await Task.WhenAny(StressCounter.AllHandled, Task.Delay(TimeSpan.FromMinutes(3)));
total.Stop();

var handled = StressCounter.HandledCount;
Console.WriteLine($"[stress] published : {count:N0} events in {publishElapsed.TotalSeconds:F2}s ({count / publishElapsed.TotalSeconds:N0} msg/s)");
if (completed == StressCounter.AllHandled)
{
    Console.WriteLine($"[stress] handled   : {handled:N0} events in {total.Elapsed.TotalSeconds:F2}s end-to-end ({handled / total.Elapsed.TotalSeconds:N0} msg/s)");
    Console.WriteLine("[stress] result    : PASS");
}
else
{
    Console.WriteLine($"[stress] TIMEOUT   : only {handled:N0}/{count:N0} events handled within 3 minutes");
    Console.WriteLine("[stress] result    : FAIL");
}

await host.StopAsync();
return completed == StressCounter.AllHandled ? 0 : 1;

public sealed record StressEvent(int Sequence);

public static class StressCounter
{
    private static int _expected;
    private static int _handled;
    private static TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static Task AllHandled => _tcs.Task;

    public static int HandledCount => Volatile.Read(ref _handled);

    public static void Reset(int expected)
    {
        _expected = expected;
        _handled = 0;
        _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public static void Increment()
    {
        if (Interlocked.Increment(ref _handled) == _expected)
        {
            _tcs.TrySetResult();
        }
    }
}

public sealed class StressHandler : IEventHandler<StressEvent>
{
    public Task HandleAsync(StressEvent @event, Headers headers, CancellationToken cancellationToken)
    {
        StressCounter.Increment();
        return Task.CompletedTask;
    }
}

public sealed class StressProfile : EventProfile
{
    public StressProfile()
    {
        this.On<StressEvent>().HandledBy<StressHandler>();
    }
}
