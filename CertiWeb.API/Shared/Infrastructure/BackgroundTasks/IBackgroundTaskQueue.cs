using System.Threading;
using System.Threading.Tasks;

namespace CertiWeb.API.Shared.Infrastructure.BackgroundTasks;

/// <summary>
/// Simple background task queue contract inspired by ASP.NET Core docs.
/// </summary>
public interface IBackgroundTaskQueue
{
    void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

    ValueTask<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}

