namespace SimpleEventBus.Event;

/// <summary>
/// The headers class
/// </summary>
/// <seealso cref="Dictionary{string,string}"/>
public class Headers : Dictionary<string,string>
{
    /// <summary>
    /// Gets or sets the Correlation ID associated with the headers.
    /// </summary>
    public string CorrelationId
    {
        get
        {
            return this.GetValueOrDefault("CorrelationId");
        }
        set
        {
            this["CorrelationId"] = value;
        }
    }

    
}