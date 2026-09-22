using FluentEventBus.AsyncApi;
using Microsoft.Extensions.DependencyInjection;
using Neuroglia.AsyncApi.IO;

namespace FluentEventBus.DependencyInjection;

/// <summary>
/// AsyncAPI registration extensions for the event bus builder.
/// </summary>
public static class AsyncApiEventBusBuilderExtensions
{
    /// <summary>
    /// Registers the <see cref="AsyncApiDocumentGenerator"/> so an AsyncAPI 3.0 document
    /// can be generated from the subscription profiles.
    /// </summary>
    public static IEventBusBuilder AddAsyncApiDocument(this IEventBusBuilder eventBusBuilder,
                                                       Action<AsyncApiDocumentOptions>? setup = null)
    {
        if (setup is not null)
        {
            eventBusBuilder.Services.Configure(setup);
        }

        eventBusBuilder.Services.AddAsyncApiIO();
        eventBusBuilder.Services.AddSingleton<AsyncApiDocumentGenerator>();

        return eventBusBuilder;
    }
}
