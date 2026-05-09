using System.Net.Sockets;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleEventBus;
using SimpleEventBus.DependencyInjection;
using SimpleEventBus.Event;
using SimpleEventBus.Profile;
using SimpleEventBus.RabbitMq;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.RabbitMqTests;

public class RabbitMqIntegrationTests
{
    private static TaskCompletionSource<bool>? CurrentTcs;

    [Fact(DisplayName = "RabbitMQ_DI_Publish_Consume_Smoke")]
    public async Task RabbitMq_DI_Publish_Consume_Smoke()
    {
        if (!IsEnabled() || !await CanConnectAsync("localhost", 5672))
        {
            return;
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        CurrentTcs = tcs;

        try
        {
            using var host = Host.CreateDefaultBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    services.AddEventBus(builder =>
                    {
                        builder.WithProfile<TestProfile>();
                        builder.ScanHandlersFrom(typeof(RabbitMqIntegrationTests).Assembly);
                        builder.UseRabbitMqTransport(
                            opt =>
                            {
                                opt.Host = "localhost";
                                opt.UserName = "guest";
                                opt.Password = "guest";
                            },
                            bind =>
                            {
                                bind.GlobalExchange = "eventbus.topic";
                                bind.GlobalQueue = "simpleeventbus.test";
                                bind.PrefetchCount = 5;
                            });
                    });
                })
                .Build();

            await host.StartAsync();

            var publisher = host.Services.GetRequiredService<IEventPublisher>();
            await publisher.PublishAsync(new TestEvent("hello"));

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await tcs.Task.WaitAsync(cts.Token);

            await host.StopAsync();

            tcs.Task.IsCompletedSuccessfully.Should().BeTrue();
        }
        finally
        {
            CurrentTcs = null;
        }

        static bool IsEnabled()
        {
            return string.Equals(Environment.GetEnvironmentVariable("RUN_RABBITMQ_TESTS"), "1", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static async Task<bool> CanConnectAsync(string host, int port)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            var timeout = Task.Delay(TimeSpan.FromSeconds(2));
            var completed = await Task.WhenAny(connectTask, timeout);
            return completed == connectTask && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private sealed record TestEvent(string Message);

    private sealed class TestHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent @event, Headers headers, CancellationToken cancellationToken)
        {
            CurrentTcs?.TrySetResult(true);
            return Task.CompletedTask;
        }
    }

    private sealed class TestProfile : SubscriptionProfile
    {
        public TestProfile()
        {
            this.WhenOccurs<TestEvent>().ToDo<TestHandler>();
        }
    }
}
