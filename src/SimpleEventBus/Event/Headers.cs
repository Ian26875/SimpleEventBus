namespace SimpleEventBus.Event;

/// <summary>
/// The headers class
/// </summary>
/// <seealso cref="Dictionary{string,string}"/>
public class Headers : Dictionary<string,object>
{
    /// <summary>
    /// Gets or sets the Correlation ID associated with the headers.
    /// </summary>
    public string CorrelationId
    {
        get
        {
            return this.GetValueOrDefault("CorrelationId")?.ToString();
        }
        set
        {
            this["CorrelationId"] = value;
        }
    }

    
}