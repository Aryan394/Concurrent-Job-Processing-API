using ConcurrentJobProcessor.Common;
using ConcurrentJobProcessor.Services;

namespace ConcurrentJobProcessor.Services;

public class JobRecoveryService(
    IServiceScopeFactory scopeFactory,
    IJobQueue jobQueue)
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IJobQueue _jobQueue = jobQueue;

    public async Task RecoverJobsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var jobStore = scope.ServiceProvider
            .GetRequiredService<IJobStore>();

        var jobs = await jobStore.GetQueuedJobsAsync();

        foreach (var job in jobs)
        {
            job.Status = JobStatus.Queued.ToString();
            job.ErrorMessage = null;

            await jobStore.UpdateAsync(job);
            await _jobQueue.EnqueueJobAsync(job);
            Console.WriteLine($"Recovered Job {job.Id}");
        }
    }
}