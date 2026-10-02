
using ConcurrentJobProcessor.Models;

namespace ConcurrentJobProcessor.Services
{
    public class JobWorker(IJobQueue jobQueue) : BackgroundService
    {
        private readonly IJobQueue _jobQueue = jobQueue;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await _jobQueue.DequeueJobAsync(stoppingToken);

                Console.WriteLine(
                    $"Processing job: {job.Id}");

                await ProcessJob(job);
            }
        }
        private static async Task ProcessJob(Job job)
        {
            await Task.Delay(5000);

            Console.WriteLine(
                $"Completed job: {job.Id}");
        }

    }
}