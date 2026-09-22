using FluentEventBus.AsyncApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Neuroglia.AsyncApi.Generation;
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
        eventBusBuilder.Services.AddSingleton<FluentEventBus.AsyncApi.AsyncApiDocumentGenerator>();

        // Consumers of IAsyncApiDocumentProvider (Neuroglia UI, serving middleware)
        // get the profile-generated document without any extra wiring.
        eventBusBuilder.Services.TryAddSingleton<IAsyncApiDocumentProvider, FluentEventBusDocumentProvider>();
        // Required by the Neuroglia AsyncAPI UI to render payload examples.
        eventBusBuilder.Services.TryAddSingleton<IJsonSchemaExampleGenerator, JsonSchemaExampleGenerator>();

        return eventBusBuilder;
    }
}
