using System.Reflection;
using SimpleEventBus.Attributes;

namespace SimpleEventBus;

internal static class TypeExtension
{
    internal static string GetEventVersion(this Type type)
    {
        var eventVersionAttribute = type.GetCustomAttributes<EventVersionAttribute>()
                                        .SingleOrDefault();
        
        return eventVersionAttribute is null ? string.Empty : eventVersionAttribute.Version;
    }
}