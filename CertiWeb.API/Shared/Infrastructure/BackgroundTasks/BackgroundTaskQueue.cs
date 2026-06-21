using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace CertiWeb.API.Shared.Infrastructure.BackgroundTasks;

/// <summary>
/// In-memory background task queue using System.Threading.Channels.
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<CancellationToken, Task>> _queue;

    public BackgroundTaskQueue(int capacity = 100)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(options);
    }

    public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
    {
        if (workItem == null) throw new ArgumentNullException(nameof(workItem));
        var written = _queue.Writer.TryWrite(workItem);
        if (!written)
        {
            // If TryWrite fails, fallback to a blocking write to apply backpressure.
            _queue.Writer.WriteAsync(workItem).GetAwaiter().GetResult();
        }
    }

    public async ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}

