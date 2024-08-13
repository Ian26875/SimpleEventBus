namespace SimpleEventBus;

[AttributeUsage(AttributeTargets.Class,Inherited = false)]
public class EventVersionAttribute : Attribute
{
    public string Version { set; get; }

    public EventVersionAttribute(string version)
    {
        Version = version;
    }

    public EventVersionAttribute(Version version)
    {
        Version = version.ToString();
    }

    public EventVersionAttribute(int major, int minor)
    {
        Version = new Version(major, minor).ToString();
    }
}