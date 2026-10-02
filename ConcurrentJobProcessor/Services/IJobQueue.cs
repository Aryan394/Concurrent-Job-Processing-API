using ConcurrentJobProcessor.Models;
namespace ConcurrentJobProcessor.Services;
public interface IJobQueue
{
    ValueTask EnqueueJobAsync(Job job);
    ValueTask<Job> DequeueJobAsync(CancellationToken cancellationToken);
}