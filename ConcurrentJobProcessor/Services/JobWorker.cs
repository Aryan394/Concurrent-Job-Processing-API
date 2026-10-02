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
                var job = await _jobQueue.DequeueJobAsync(stoppingToken);
                job.Status = "Processing";
                _jobStore.Update(job);
                Console.WriteLine($"Worker {workerId} started Job {job.Id}");
                await ProcessJob(job, workerId);
                job.Status = "Completed";
                _jobStore.Update(job);

            Console.WriteLine($"Worker {workerId} completed Job {job.Id}");
            }
        }
        private async Task ProcessJob(Job job, int workerId)
        {
            await Task.Delay(5000);
        }
    }
}