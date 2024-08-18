namespace SimpleEventBus.Attributes;

/// <summary>
/// The event version attribute class
/// </summary>
/// <seealso cref="Attribute"/>
[AttributeUsage(AttributeTargets.Class,Inherited = false)]
public class EventVersionAttribute : Attribute
{
    /// <summary>
    /// Sets or gets the value of the version
    /// </summary>
    public string Version { set; get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventVersionAttribute"/> class
    /// </summary>
    /// <param name="version">The version</param>
    public EventVersionAttribute(string version)
    {
        Version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventVersionAttribute"/> class
    /// </summary>
    /// <param name="version">The version</param>
    public EventVersionAttribute(Version version)
    {
        Version = version.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventVersionAttribute"/> class
    /// </summary>
    /// <param name="major">The major</param>
    /// <param name="minor">The minor</param>
    public EventVersionAttribute(int major, int minor)
    {
        Version = new Version(major, minor).ToString();
    }
}