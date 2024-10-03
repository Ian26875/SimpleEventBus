namespace SimpleEventBus.Schema;

/// <summary>
/// The event version attribute class
/// </summary>
/// <seealso cref="Attribute"/>
[AttributeUsage(AttributeTargets.Class,Inherited = false)]
public class EventAttribute : Attribute
{
    /// <summary>
    /// Sets or gets the value of the version
    /// </summary>
    public string Version { set; get; }

    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventAttribute"/> class
    /// </summary>
    /// <param name="version">The version</param>
    public EventAttribute(string version)
    {
        Version = version;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventAttribute"/> class
    /// </summary>
    /// <param name="version">The version</param>
    public EventAttribute(Version version)
    {
        Version = version.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventAttribute"/> class
    /// </summary>
    /// <param name="major">The major</param>
    /// <param name="minor">The minor</param>
    public EventAttribute(int major, int minor)
    {
        Version = new Version(major, minor).ToString();
    }
}