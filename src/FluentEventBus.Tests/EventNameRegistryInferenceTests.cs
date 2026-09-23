using FluentAssertions;
using FluentEventBus.Naming;
namespace FluentEventBus.Tests
{
    public class EventNameRegistryInferenceTests
    {
        [Fact]
        public void GetEventName_ShouldHandleAcronyms()
        {
            var name = new EventNameRegistry().GetEventName(typeof(MapperCases.HTTPServerStarted));
            name.Should().Be("mappercases.http.server.started.v1");
        }

        [Fact]
        public void GetEventName_ShouldHandleNumbersAndAcronyms()
        {
            var name = new EventNameRegistry().GetEventName(typeof(MapperCases.Order2FAEnabled));
            name.Should().Be("mappercases.order.2.fa.enabled.v1");
        }

        [Fact]
        public void GetEventName_ShouldHandleSingleToken()
        {
            var name = new EventNameRegistry().GetEventName(typeof(MapperCases.Ping));
            name.Should().Be("mappercases.ping.event.v1");
        }

        [Fact]
        public void GetEventName_ShouldRespectExplicitDottedName()
        {
            var name = new EventNameRegistry().GetEventName(typeof(MapperCases.BillingInvoicePaid));
            name.Should().Be("billing.invoice.paid.v2");
        }
    }
}

namespace FluentEventBus.Tests.MapperCases
{
    public sealed class HTTPServerStarted { }
    public sealed class Order2FAEnabled { }
    public sealed class Ping { }

    [Event("billing.invoice.paid", 2)]
    public sealed class BillingInvoicePaid { 
        [Fact(DisplayName = "Registries_OfDifferentInstances_AreFullyIsolated")]
        public void Registries_OfDifferentInstances_AreFullyIsolated()
        {
            var first = new EventNameRegistry();
            var second = new EventNameRegistry();

            first.Register(typeof(MapperCases.Ping));

            var resolved = first.GetEventType(first.GetEventName(typeof(MapperCases.Ping)));
            resolved.Should().Be(typeof(MapperCases.Ping));

            var act = () => second.GetEventType(first.GetEventName(typeof(MapperCases.Ping)));
            act.Should().Throw<KeyNotFoundException>();
        }
}
}
