using Sale_Saas.Domain.Entities;

public static class TrackingLogQueue
{
    private static readonly Queue<TrackingLog> _queue = new Queue<TrackingLog>();
    private static readonly object _lock = new object();

    public static void Enqueue(TrackingLog trackingLog)
    {
        lock (_lock)
        {
            _queue.Enqueue(trackingLog);
        }
    }

    public static TrackingLog? Dequeue()
    {
        lock (_lock)
        {
            return _queue.Count > 0 ? _queue.Dequeue() : null;
        }
    }

    public static bool IsEmpty()
    {
        lock (_lock)
        {
            return _queue.Count == 0;
        }
    }
}
