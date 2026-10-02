using ConcurrentJobProcessor.Common;
using ConcurrentJobProcessor.Models;
using Microsoft.Extensions.Hosting;

namespace ConcurrentJobProcessor.Services
{
    public class JobWorker(IJobQueue jobQueue) : BackgroundService
    {
        private readonly IJobQueue _jobQueue = jobQueue;

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
                Console.WriteLine($"Worker {workerId} started Job {job.Id}");
                await ProcessJob(job, workerId);

            Console.WriteLine($"Worker {workerId} completed Job {job.Id}");
            }
        }
        private async Task ProcessJob(Job job, int workerId)
        {
            await Task.Delay(5000);
        }
    }
}