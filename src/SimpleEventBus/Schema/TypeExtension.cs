using System.Reflection;

namespace SimpleEventBus.Schema;

internal static class TypeExtension
{
    internal static string GetEventVersion(this Type type)
    {
        var eventVersionAttribute = type.GetCustomAttributes<EventVersionAttribute>()
                                        .SingleOrDefault();
        
        return eventVersionAttribute is null ? string.Empty : eventVersionAttribute.Version;
    }

    internal static TAttribute GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        return type.GetCustomAttributes<TAttribute>().SingleOrDefault();
    }
}