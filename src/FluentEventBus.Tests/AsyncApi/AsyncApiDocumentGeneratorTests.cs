#if NET8_0_OR_GREATER
using FluentAssertions;
using FluentEventBus.AsyncApi;
using FluentEventBus.DependencyInjection;
using FluentEventBus.Event;
using FluentEventBus.Profile;
using FluentEventBus.Schema;
using FluentEventBus.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi;

namespace FluentEventBus.Tests.AsyncApi;

public class AsyncApiDocumentGeneratorTests
{
    [Fact(DisplayName = "Generate_ShouldBuildChannelsAndReceiveOperationsFromProfiles")]
    public void Generate_ShouldBuildChannelsAndReceiveOperationsFromProfiles()
    {
        var generator = BuildGenerator();

        var document = generator.Generate();

        document.AsyncApi.Should().Be("3.0.0");
        document.Info.Title.Should().Be("Orders Service");
        document.Channels.Should().ContainKey("OrderShipped");
        document.Channels["OrderShipped"].Address.Should().Be("OrderShipped");
        document.Channels["OrderShipped"].Messages.Should().ContainKey(nameof(OrderShipped));
        document.Operations.Should().ContainKey("receive.OrderShipped");
        document.Operations["receive.OrderShipped"].Channel.Reference.Should().Be("#/channels/OrderShipped");
    }

    [Fact(DisplayName = "SerializeAsync_ShouldProduceJsonDocument")]
    public async Task SerializeAsync_ShouldProduceJsonDocument()
    {
        var generator = BuildGenerator();

        var json = await generator.SerializeAsync(AsyncApiDocumentFormat.Json);

        json.Should().Contain("\"asyncapi\"").And.Contain("3.0.0").And.Contain("OrderShipped");
    }

    [Fact(DisplayName = "Generate_WithServerDescriptor_ShouldDeclareServerFromTransport")]
    public void Generate_WithServerDescriptor_ShouldDeclareServerFromTransport()
    {
        var generator = BuildGenerator(services: services =>
            services.AddSingleton<IEventBusServerDescriptor>(
                new EventBusServerDescriptor("rabbitmq", "broker:5672", "amqp", "0.9.1")));

        var document = generator.Generate();

        document.Servers.Should().ContainKey("rabbitmq");
        document.Servers!["rabbitmq"].Host.Should().Be("broker:5672");
        document.Servers["rabbitmq"].Protocol.Should().Be("amqp");
    }

    [Fact(DisplayName = "Generate_ExplicitWithServer_ShouldOverrideDescriptor")]
    public void Generate_ExplicitWithServer_ShouldOverrideDescriptor()
    {
        var generator = BuildGenerator(
            options => options.WithServer("rabbitmq", "override:5672", "amqp"),
            services => services.AddSingleton<IEventBusServerDescriptor>(
                new EventBusServerDescriptor("rabbitmq", "broker:5672", "amqp")));

        var document = generator.Generate();

        document.Servers!["rabbitmq"].Host.Should().Be("override:5672");
    }

    [Fact(DisplayName = "Generate_WithExample_ShouldIncludeMessageExample")]
    public void Generate_WithExample_ShouldIncludeMessageExample()
    {
        var generator = BuildGenerator(options =>
            options.WithExample(new OrderShipped(Guid.NewGuid(), "DHL"), name: "sample", summary: "A shipped order."));

        var document = generator.Generate();

        var examples = document.Channels["OrderShipped"].Messages![nameof(OrderShipped)].Examples;
        examples.Should().NotBeNull().And.HaveCount(1);
        examples![0].Name.Should().Be("sample");
        examples[0].Payload.Should().ContainKey("Carrier");
    }

    private static AsyncApiDocumentGenerator BuildGenerator(Action<AsyncApiDocumentOptions>? extraSetup = null,
                                                            Action<IServiceCollection>? services = null)
    {
        var serviceCollection = new ServiceCollection();
        services?.Invoke(serviceCollection);
        BuildGeneratorCore(serviceCollection, extraSetup);
        return serviceCollection.BuildServiceProvider().GetRequiredService<AsyncApiDocumentGenerator>();
    }

    private static void BuildGeneratorCore(ServiceCollection services, Action<AsyncApiDocumentOptions>? extraSetup)
    {
        services.AddLogging();
        services.AddEventBus(builder =>
        {
            builder.WithProfile<ShippingProfile>();
            builder.AddAsyncApiDocument(options =>
            {
                options.Title = "Orders Service";
                options.Version = "1.2.3";
                extraSetup?.Invoke(options);
            });
        });
        services.AddSingleton<IEventMapper>(new StubEventMapper());
    }

    private sealed record OrderShipped(Guid OrderId, string Carrier);

    private sealed class OrderShippedHandler : IEventHandler<OrderShipped>
    {
        public Task HandleAsync(OrderShipped @event, Headers headers, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class ShippingProfile : SubscriptionProfile
    {
        public ShippingProfile()
        {
            this.WhenOccurs<OrderShipped>().ToDo<OrderShippedHandler>();
        }
    }

    private sealed class StubEventMapper : IEventMapper
    {
        private readonly Dictionary<string, Type> _types = new();

        public void Register(Type eventType) => _types[eventType.Name] = eventType;

        public string GetEventName(Type type) => type.Name;

        public Type GetEventType(string type) => _types[type];
    }
}
#endif
