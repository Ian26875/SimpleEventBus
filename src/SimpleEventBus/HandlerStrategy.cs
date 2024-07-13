namespace SimpleEventBus;

/// <summary>
/// The handler strategy enum
/// </summary>
public enum HandlerStrategy
{
    /// <summary>
    /// The for each handler strategy
    /// </summary>
    ForEach,
    
    /// <summary>
    /// The task when all handler strategy
    /// </summary>
    TaskWhenAll
}