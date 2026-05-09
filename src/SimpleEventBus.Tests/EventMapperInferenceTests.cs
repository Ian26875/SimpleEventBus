using FluentAssertions;
using SimpleEventBus.Mapper;
using SimpleEventBus.Schema;
namespace SimpleEventBus.Tests
{
    public class EventMapperInferenceTests
    {
        [Fact]
        public void GetEventName_ShouldHandleAcronyms()
        {
            var name = EventMapper.Instance.GetEventName(typeof(MapperCases.HTTPServerStarted));
            name.Should().Be("mappercases.http.server.started.v1");
        }

        [Fact]
        public void GetEventName_ShouldHandleNumbersAndAcronyms()
        {
            var name = EventMapper.Instance.GetEventName(typeof(MapperCases.Order2FAEnabled));
            name.Should().Be("mappercases.order.2.fa.enabled.v1");
        }

        [Fact]
        public void GetEventName_ShouldHandleSingleToken()
        {
            var name = EventMapper.Instance.GetEventName(typeof(MapperCases.Ping));
            name.Should().Be("mappercases.ping.event.v1");
        }

        [Fact]
        public void GetEventName_ShouldRespectExplicitDottedName()
        {
            var name = EventMapper.Instance.GetEventName(typeof(MapperCases.BillingInvoicePaid));
            name.Should().Be("billing.invoice.paid.v2");
        }
    }
}

namespace SimpleEventBus.Tests.MapperCases
{
    public sealed class HTTPServerStarted { }
    public sealed class Order2FAEnabled { }
    public sealed class Ping { }

    [Event("billing.invoice.paid", 2)]
    public sealed class BillingInvoicePaid { }
}
