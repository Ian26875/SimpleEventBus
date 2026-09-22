using SimpleEventBus.Event;

namespace SimpleEventBus.InMemory;

public class BackgroundQueueOptions
{
    /// <summary>
    ///     最大佇列容量
    /// </summary>
    public int Capacity { get; set; } = 10000;

    /// <summary>
    ///     告警閾值，超過此值可觸發警示
    /// </summary>
    public int AlertThreshold { get; set; } = 8000;

    /// <summary>
    ///     當超過警戒值時的處理行為
    /// </summary>
    public Func<Task>? OnAlert { get; set; }

    /// <summary>
    ///     失敗事件重新入列的次數上限，超過即視為 poison message
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    ///     Poison message（重試超限）的處理行為；未設定時僅記錄 error log
    /// </summary>
    public Func<EventContext, Exception, Task>? OnPoisonMessage { get; set; }
}