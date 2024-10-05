namespace SimpleEventBus.Schema;

/// <summary>
/// The event version attribute class
/// </summary>
/// <seealso cref="Attribute"/>
[AttributeUsage(AttributeTargets.Class,Inherited = false)]
public sealed class EventAttribute : Attribute
{
    /// <summary>
    /// The default version
    /// </summary>
    public const int DefaultVersion = 1;
    
    /// <summary>
    /// Sets or gets the value of the version
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the value of the name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventAttribute"/> class
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="major">The major</param>
    public EventAttribute(string name, int major = DefaultVersion)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }
        
        if (major <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(major));
        }
        
        Name = name;
        Version = major.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventAttribute"/> class
    /// </summary>
    /// <param name="major">The major</param>
    public EventAttribute(int major)
    {
        if (major <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(major));
        }
        Name = string.Empty;
        Version = major.ToString();
    }
}