using ConcurrentJobProcessor.Common;
using ConcurrentJobProcessor.Models;
using Microsoft.Extensions.Hosting;

namespace ConcurrentJobProcessor.Services
{
    public class JobWorker(IJobQueue jobQueue, IJobStore jobStore) : BackgroundService
    {
        private readonly IJobQueue _jobQueue = jobQueue;
        private readonly IJobStore _jobStore = jobStore;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workers = Enumerable.Range(1, Constants.WorkerCount)
            .Select(workerId => ProcessJobs(workerId, stoppingToken))
            .ToArray();

            await Task.WhenAll(workers);
        }

        private async Task ProcessJobs(int workerId, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Job? job = null;
                try
                {
                    job = await _jobQueue.DequeueJobAsync(stoppingToken);
                    job.Status = JobStatus.Processing.ToString();
                    job.StartedAt = DateTime.UtcNow;
                    _jobStore.Update(job);
                    Console.WriteLine($"Worker {workerId} started Job {job.Id}");
                    await ProcessJob(job, workerId);
                    job.Status = JobStatus.Completed.ToString();
                    _jobStore.Update(job);
                    Console.WriteLine($"Worker {workerId} completed Job {job.Id}");
                }
                catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (job == null)
                    {
                        Console.WriteLine($"Worker {workerId} encountered an error: {ex.Message}");
                        continue;
                    }

                    job.Status = JobStatus.Failed.ToString();
                    job.ErrorMessage = ex.Message;
                    _jobStore.Update(job);
                    Console.WriteLine(
                        $"Worker {workerId} failed Job {job.Id}: {ex.Message}");
                }
            }
        }
        private async Task ProcessJob(Job job, int workerId)
        {
            await Task.Delay(5000);
        }
    }
}